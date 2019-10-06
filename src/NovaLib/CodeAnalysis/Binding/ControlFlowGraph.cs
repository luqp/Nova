using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nova.CodeAnalysis.Symbols;
using Nova.CodeAnalysis.Syntax;

namespace Nova.CodeAnalysis.Binding
{
    internal sealed class ControlFlowGraph
    {
        private ControlFlowGraph(BasicBlock start, BasicBlock end, List<BasicBlock> blocks, List<BasicBlockBranch> branches)
        {
            Start = start;
            End = end;
            Blocks = blocks;
            Branches = branches;
        }

        public BasicBlock Start { get; }
        public BasicBlock End { get; }
        public List<BasicBlock> Blocks { get; }
        public List<BasicBlockBranch> Branches { get; }

        public sealed class BasicBlock
        {
            public BasicBlock()
            {                
            }

            public BasicBlock(bool isStart)
            {
                IsStart = isStart;
                IsEnd = !isStart;
            }

            public bool IsStart { get; }
            public bool IsEnd { get; }
            public List<BoundStatement> Statements { get; } = new List<BoundStatement>();
            public List<BasicBlockBranch> Incoming { get; } = new List<BasicBlockBranch>();
            public List<BasicBlockBranch> Outgoing { get; } = new List<BasicBlockBranch>();

            public override string ToString()
            {
                if (IsStart)
                    return "<Start>";

                if (IsEnd)
                    return "<End>";
                
                using (StringWriter writer = new StringWriter())
                {
                    foreach(BoundStatement statement in Statements)
                        statement.WriteTo(writer);
                    
                    return writer.ToString();
                }
            }
        }

        public sealed class BasicBlockBranch
        {
            public BasicBlockBranch(BasicBlock from, BasicBlock to,  BoundExpression condition)
            {
                From = from;
                To = to;
                Condition = condition;
            }

            public BasicBlock From { get; }
            public BasicBlock To { get; }
            public BoundExpression Condition { get; }

            public override string ToString()
            {
                if (Condition == null)
                    return string.Empty;
                
                return Condition.ToString();
            }
        }

        public sealed class BasicBlockBuilder
        {
            private List<BoundStatement> statements = new List<BoundStatement>();
            private List<BasicBlock> blocks = new List<BasicBlock>();

            public List<BasicBlock> Build(BoundBlockStatement block)
            {
                foreach (BoundStatement statement in block.Statements)
                {
                    switch (statement.Kind)
                    {
                        case BoundNodeKind.LabelStatement:
                            StartBlock();
                            statements.Add(statement);
                            break;
                        case BoundNodeKind.GotoStatement:
                        case BoundNodeKind.ConditionalGotoStatement:
                        case BoundNodeKind.ReturnStatement:
                            statements.Add(statement);
                            StartBlock();
                            break;
                        case BoundNodeKind.VariableDeclaration:
                        case BoundNodeKind.ExpressionStatement:
                            statements.Add(statement);
                            break;
                        default:
                            throw new Exception($"Unexpected statemend: '{statement.Kind}'.");
                    }
                }

                EndBlock();

                return blocks.ToList();
            }

            private void StartBlock()
            {
                EndBlock();
            }

            private void EndBlock()
            {
                if (statements.Count > 0)
                {
                    BasicBlock block = new BasicBlock();
                    block.Statements.AddRange(statements);
                    blocks.Add(block);
                    statements.Clear();
                }
            }
        }

        public sealed class GraphBuilder
        {
            private Dictionary<BoundStatement, BasicBlock> blockFromStatement = new Dictionary<BoundStatement, BasicBlock>();
            private Dictionary<BoundLabel, BasicBlock> blockFromLabel = new Dictionary<BoundLabel, BasicBlock>();
            private List<BasicBlockBranch> branches = new List<BasicBlockBranch>();
            private BasicBlock start = new BasicBlock(isStart: true);
            private BasicBlock end = new BasicBlock(isStart: false);

            public ControlFlowGraph Build(List<BasicBlock> blocks)
            {
                if (!blocks.Any())
                    Connect(start, end);
                else
                    Connect(start, blocks.First());

                foreach (BasicBlock block in blocks)
                {
                    foreach (var statement in block.Statements)
                    {
                        blockFromStatement.Add(statement, block);
                        if (statement is BoundLabelStatement labelStatement)
                            blockFromLabel.Add(labelStatement.Label, block);
                    }
                }

                for (int i = 0; i < blocks.Count; i++)
                {
                    BasicBlock current = blocks[i];
                    var next = i == blocks.Count - 1 ? end : blocks[i + 1];

                    foreach (var statement in current.Statements)
                    {
                        bool isLastStatementInBlock = statement == current.Statements.Last();
                        switch (statement.Kind)
                        {
                            case BoundNodeKind.GotoStatement:
                                BoundGotoStatement gs = (BoundGotoStatement)statement;
                                BasicBlock toBlock = blockFromLabel[gs.Label];
                                Connect(current, toBlock);
                                break;
                            case BoundNodeKind.ConditionalGotoStatement:
                                BoundConditionalGotoStatement cgs = (BoundConditionalGotoStatement)statement;
                                BasicBlock themBlock = blockFromLabel[cgs.Label];
                                BasicBlock elseBlock = next;
                                BoundExpression negatedCondition = Negate(cgs.Condition);
                                BoundExpression thenCondition = cgs.JumpIfTrue ? cgs.Condition: negatedCondition;
                                BoundExpression elseCondition = cgs.JumpIfTrue ? negatedCondition: cgs.Condition;
                                Connect(current, themBlock, thenCondition);
                                Connect(current, elseBlock, elseCondition);
                                break;
                            case BoundNodeKind.ReturnStatement:
                                Connect(current, end);
                                break;
                            case BoundNodeKind.LabelStatement:
                            case BoundNodeKind.VariableDeclaration:
                            case BoundNodeKind.ExpressionStatement:
                                if (isLastStatementInBlock)
                                    Connect(current, next);
                                break;
                            default:
                                throw new Exception($"Unexpected statemend: '{statement.Kind}'.");
                        }
                    }
                }

            ScanAgain:
                foreach (BasicBlock block in blocks)
                {
                    if (!block.Incoming.Any())
                    {
                        RemoveBlock(blocks, block);
                        goto ScanAgain;
                    }
                }

                blocks.Insert(0, start);
                blocks.Add(end);

                return new ControlFlowGraph(start, end, blocks, branches);
            }

            private void Connect(BasicBlock from, BasicBlock to, BoundExpression condition = null)
            {
                if (condition is BoundLiteralExpression l)
                {
                    bool value = (bool) l.Value;
                    if (value)
                        condition = null;
                    else
                        return;
                }

                BasicBlockBranch branch = new BasicBlockBranch(from, to, condition);
                from.Outgoing.Add(branch);
                to.Incoming.Add(branch);
                branches.Add(branch);
            }

            private void RemoveBlock(List<BasicBlock> blocks, BasicBlock block)
            {
                foreach (var branch in block.Incoming)
                {
                    branch.From.Outgoing.Remove(branch);
                    branches.Remove(branch);
                }

                foreach (var branch in block.Outgoing)
                {
                    branch.To.Incoming.Remove(branch);
                    branches.Remove(branch);
                }

                blocks.Remove(block);
            }

            private BoundExpression Negate(BoundExpression condition)
            {
                if (condition is BoundLiteralExpression literal)
                {
                    bool value = (bool) literal.Value;
                    return new BoundLiteralExpression(!value);
                }

                var op = BoundUnaryOperator.Bind(SyntaxKind.BangToken, TypeSymbol.Bool);
                return new BoundUnaryExpression(op, condition);
            }
        }

        public void WriteTo(TextWriter writer)
        {
            string Quote(string text)
            {
                return "\"" + text.Replace("\"", "\\\"") + "\"";
            }

            writer.WriteLine("digraph G {");

            Dictionary<BasicBlock, string> blockIds = new Dictionary<BasicBlock, string>();

            for (int i = 0; i < Blocks.Count; i++)
            {
                string id = $"N{i}";
                blockIds.Add(Blocks[i], id);
            }

            foreach (BasicBlock block in Blocks)
            {
                string id = blockIds[block];
                string label = Quote(block.ToString().Replace(Environment.NewLine, "\\l"));
                writer.WriteLine($"    {id} [label = {label} shape = box]");
            }

            foreach (BasicBlockBranch branch in Branches)
            {
                string fromId = blockIds[branch.From];
                string toId = blockIds[branch.To];
                string label = Quote(branch.ToString());
                writer.WriteLine($"    {fromId} -> {toId} [label = {label}]");
            }

            writer.WriteLine("}");
        }

        public static ControlFlowGraph Create(BoundBlockStatement body)
        {
            BasicBlockBuilder basicBlockBuilder = new BasicBlockBuilder();
            List<BasicBlock> blocks = basicBlockBuilder.Build(body);
            
            GraphBuilder graphBuilder = new GraphBuilder();
            return graphBuilder.Build(blocks);
        }
    
        public static bool AllPathsReturn(BoundBlockStatement body)
        {
            ControlFlowGraph graph = Create(body);

            foreach (BasicBlockBranch branch in graph.End.Incoming)
            {
                var lastStatement = branch.From.Statements.Last();
                if (lastStatement.Kind != BoundNodeKind.ReturnStatement)
                    return false;
            }

            return true;
        }
    }
}