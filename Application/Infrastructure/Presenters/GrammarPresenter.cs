using Application.Models.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Presenters
{
    public class GrammarPresenter : IPresenterVisitor
    {
        public void Visit(ProgramRoot node, int depth = 0)
        {
            write(depth, "PROGRAM");

            foreach (var function in node.FunctionDeclaration)
            {
                function.Accept(this, depth + 1);
            }
        }

        public void Visit(FunctionDecl functionDecl, int depth)
        {
            write(depth, $"FUNCTION {functionDecl.Type} {functionDecl.Name}");

            foreach (var param in functionDecl.Parameters)
            {
                param.Accept(this, depth + 1);
            }

            functionDecl.Block.Accept(this, depth + 1);
        }

        public void Visit(BlockStmt node, int depth)
        {
            write(depth, $"BLOCK");

            foreach (var statement in node.Statements)
            {
                statement.Accept(this, depth + 1);
            }
        }

        public void Visit(IdentifierAssignmentStatement node, int depth)
        {
            write(depth, $"IDENT ASSIGN: {node.Identifier.Name}");
            node.Expression.Accept(this, depth + 1);
        }

        public void Visit(PropertyAssignmentStatement node, int depth)
        {
            write(depth, $"PROP ASSIGN: {node.Property}");
            node.Expression.Accept(this, depth + 1);
        }

        public void Visit(IndexAssignmentStatement node, int depth)
        {
            write(depth, $"INDEX ASSIGN");
            node.IndexExpr.Accept(this, depth + 1);
            node.Expression.Accept(this, depth + 1);
        }

        public void Visit(DeclarationStmt node, int depth)
        {
            write(depth, $"DECLARATION: {node.Type} {node.Identifier}");

            if (node.Expression != null)
            {
                write(depth, $"DECLARATION EXPRESSION");
                node.Expression!.Accept(this, depth + 1);
            }
        }

        public void Visit(ReturnStmt node, int depth)
        {
            write(depth, $"RETURN");
            node.ReturnExpression.Accept(this, depth + 1);
        }

        public void Visit(WhileStmt node, int depth)
        {
            write(depth, $"WHILE CONDITION");
            node.Condition.Accept(this, depth + 1);

            write(depth, $"WHILE STATEMENT");
            node.Statement.Accept(this, depth + 1);
        }

        public void Visit(IfStmt node, int depth)
        {
            write(depth, $"IF");
            node.Condition.Accept(this, depth + 1);

            write(depth, $"THEN");
            node.ThenStatement.Accept(this, depth + 1);

            if (node.ElseStatement != null)
            {
                write(depth, $"ELSE");
                node.Accept(this, depth + 1);
            }
        }

        public void Visit(ForeachStmt node, int depth)
        {
            write(depth, $"FOREACH {node.Parameter.Type} {node.Parameter.Identifier}");
            node.CollectionExpression.Accept(this, depth + 1);

            write(depth, $"FOREACH STATEMENT");
            node.Statement.Accept(this, depth + 1);
        }

        public void Visit(FinancialToStmt node, int depth)
        {
            write(depth, $"FINANCIAL TO  {node.Operator.ToString()}");
            node.AccountExpression.Accept(this, depth + 1);

            write(depth + 1, $"FINANCIAL TO  VALUE");
            node.ValueExpression.Accept(this, depth + 1);
        }

        public void Visit(FinancialFromStmt node, int depth)
        {
            write(depth, $"FINANCIAL FROM {node.Operator.ToString()}");
            node.AccountFromExpression.Accept(this, depth + 1);

            write(depth + 1, $"FINANCIAL FROM VALUE");
            node.ValueExpression.Accept(this, depth + 1);

            if (node.AccountToExpression != null)
            {
                write(depth + 1, $"FINANCIAL FROM TO ACCOUNT");
                node.AccountToExpression.Accept(this, depth + 1);
            }
        }

        public void Visit(Parameter node, int depth)
        {
            write(depth, $"PARAMETER {node.Type} {node.Identifier}");
        }

        public void Visit(Lambda node, int depth)
        {
            write(depth, $"LAMBDA");
            node.Parameter.Accept(this, depth + 1);
            node.Stmt.Accept(this, depth + 1);
        }

        public void Visit(Identifier node, int depth)
        {
            write(depth, $"IDENTIFIER {node.Name}");
        }

        public void Visit(ExpressionArgument node, int depth)
        {
            write(depth, $"ARGUMENT");
            node.Expression.Accept(this, depth + 1);
        }

        public void Visit(ExpressionStmt node, int depth)
        {
            write(depth, $"EXPRESSSION STATEMET");
            node.RightExpression.Accept(this, depth + 1);
        }

        public void Visit(AndExpr node, int depth)
        {
            write(depth, $"AND");
            node.FirstOperand.Accept(this, depth + 1);

            foreach (var item in node.Operands)
            {
                item.Accept(this, depth + 1);
            }
        }

        public void Visit(OrExpr node, int depth)
        {
            write(depth, $"OR");
            node.FirstOperand.Accept(this, depth + 1);

            foreach (var item in node.Operands)
            {
                item.Accept(this, depth + 1);
            }
        }

        public void Visit(AdditiveExpr node, int depth)
        {
            write(depth, $"ADD");
            node.FirstOperand.Accept(this, depth + 1);

            foreach (var item in node.Operands)
            {
                write(depth + 1, $"{item.Item1}");
                item.Item2.Accept(this, depth + 1);
            }
        }

        public void Visit(MultiplicativeExpr node, int depth)
        {
            write(depth, $"MUL");
            node.FirstOperand.Accept(this, depth + 1);

            foreach (var item in node.Operands)
            {
                write(depth + 1, $"{item.Item1}");
                item.Item2.Accept(this, depth + 1);
            }
        }

        public void Visit(NegativeExpr node, int depth)
        {
            write(depth, $"NEG {node.Operator.ToString()}");
            node.Operand.Accept(this, depth + 1);
        }

        public void Visit(ConversionExpr node, int depth)
        {
            write(depth, $"CONVERSION");
            node.OryginalExpression.Accept(this, depth + 1);

            write(depth + 1, $"TO");
            node.TypeExpression.Accept(this, depth + 1);
        }


        public void Visit(FunctionCallExpr node, int depth)
        {
            write(depth, $"FUNCTION CALL {node.Name}");

            foreach (var item in node.Arguments)
            {
                item.Accept(this, depth + 1);
            }
        }

        public void Visit(ComparativeExpr node, int depth)
        {
            write(depth, $"COMPARE");
            node.FirstOperand.Accept(this, depth + 1);

            foreach (var item in node.Operands)
            {
                write(depth + 1, $"{item.Item1}");
                item.Item2.Accept(this, depth + 1);
            }
        }

        public void Visit(PrctOfExpr node, int depth)
        {
            write(depth, $"PRCT");
            node.FirstOperand.Accept(this, depth + 1);

            write(depth + 1, $"OF");
            node.SecondOperand.Accept(this, depth + 1);
        }

        public void Visit(ConstructiorCallExpr node, int depth)
        {
            write(depth, $"CONSTRUCTOR {node.Type}");

            foreach (var item in node.Arguments)
            {
                item.Accept(this, depth + 1);
            }
        }

        public void Visit(ObjectPropertyExpr node, int depth)
        {
            write(depth, $"PROPERTY {node.Property} OF");
            node.Object.Accept(this, depth + 1);
        }

        public void Visit(ObjectIndexExpr node, int depth)
        {
            write(depth, $"INDEX OF");
            node.Object.Accept(this, depth + 1);

            write(depth + 1, $"INDEX EXPR");
            node.IndexExpression.Accept(this, depth + 1);
        }

        public void Visit(ObjectMethodExpr node, int depth)
        {
            write(depth, $"METHOD {node.Method} OF");
            node.Object.Accept(this, depth + 1);

            foreach (var item in node.Arguments)
            {
                write(depth, "ARGUMENT");
                item.Accept(this, depth + 1);
            }
        }

        public void Visit(BracedExprTerm node, int depth)
        {
            write(depth, $"BRACED");
            node.Expression.Accept(this, depth + 1);
        }

        public void Visit(Literal node, int depth)
        {
            write(depth, $"LITERAL {node.Type} VALUE {(Object)node.BoolValue! ?? (Object)node.IntValue! ?? (Object)node.StringValue! ?? (Object)node.DecimalValue!}");
        }

        private static void writeIndentation(int depth)
        {
            Console.Write(Enumerable.Repeat(' ', depth).ToArray());
        }

        private void write(int depth, string text)
        {
            writeIndentation(depth);
            Console.WriteLine(text);
        }
    }
}
