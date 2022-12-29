using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Presenters
{
    public class GrammarPresenter : IVisitor
    {
        private int depth;

        public void Visit(ProgramRoot node)
        {
            depth = 0;

            write("PROGRAM");

            foreach (var function in node.FunctionDeclarations.Values)
            {
                accept(function);
            }
        }

        public void Visit(FunctionDecl functionDecl)
        {
            write($"FUNCTION {functionDecl.Name}");

            push();
            if (functionDecl.Type != null)
            {
                write($"RETURN TYPE");
                accept(functionDecl.Type);
            }
            else
            {
                write($"RETURN TYPE is VOID");
            }

            foreach (var param in functionDecl.Parameters)
            {
                param.Accept(this);
            }

            accept(functionDecl.Block);

            pop();
        }

        public void Visit(BlockStmt node)
        {
            write($"BLOCK");

            foreach (var statement in node.Statements)
            {
                accept(statement);
            }
        }

        public void Visit(IdentifierAssignmentStatement node)
        {
            write($"IDENT ASSIGN: {node.Identifier.Name}");
            accept(node.Expression);
        }

        public void Visit(PropertyAssignmentStatement node)
        {
            write($"PROP ASSIGN");

            push();
            write($"PROPERTY");
            accept(node.Property);
            write($"VALUE");
            accept(node.Expression);
            pop();
        }

        public void Visit(IndexAssignmentStatement node)
        {
            write($"INDEX ASSIGN");
            push();
            write($"OBJECT[INDEX]");
            accept(node.IndexExpr);
            write($"VALUE");
            accept(node.Expression);
            pop();
        }

        public void Visit(DeclarationStmt node)
        {
            write($"DECLARATION {node.Identifier.Name}");

            push();
            if (node.Type != null)
            {
                write($"DECLARATION TYPE");
                accept(node.Type!);
            }
            else
            {
                write($"DECLARATION TYPE is VAR");
            }

            if (node.Expression != null)
            {
                write($"DECLARATION EXPRESSION");
                accept(node.Expression!);
            }
            pop();
        }

        public void Visit(ReturnStmt node)
        {
            write($"RETURN");

            if (node.ReturnExpression != null)
            {
                accept(node.ReturnExpression);
            }
            else
            {
                push();
                write($"RETURN VOID");
                pop();
            }
        }

        public void Visit(WhileStmt node)
        {
            write($"WHILE CONDITION");
            accept(node.Condition);

            write($"WHILE STATEMENT");
            accept(node.Statement);
        }

        public void Visit(IfStmt node)
        {
            write($"IF");
            accept(node.Condition);

            write($"THEN");
            accept(node.ThenStatement);

            if (node.ElseStatement != null)
            {
                write($"ELSE");
                accept(node.ElseStatement);
            }
        }

        public void Visit(ForeachStmt node)
        {
            write($"FOREACH");

            push();
            write($"FOREACH PARAMETER");
            accept(node.Parameter);

            write($"FOREACH COLLECTION");
            accept(node.CollectionExpression);

            write($"FOREACH STATEMENT");
            accept(node.Statement);
            pop();
        }

        public void Visit(FinancialToStmt node)
        {
            write($"FINANCIAL TO  {node.Operator.ToString()}");
            accept(node.AccountExpression);

            write($"FINANCIAL TO  VALUE");
            accept(node.ValueExpression);
        }

        public void Visit(FinancialFromStmt node)
        {
            write($"FINANCIAL FROM {node.Operator.ToString()}");
            accept(node.AccountFromExpression);

            push();
            write($"FINANCIAL FROM VALUE");
            accept(node.ValueExpression);

            if (node.AccountToExpression != null)
            {
                write($"FINANCIAL FROM TO ACCOUNT");
                accept(node.AccountToExpression);
            }
            pop();
        }

        public void Visit(Parameter node)
        {
            write($"PARAMETER {node.Identifier}");
            push();
            write($"PARAMETER TYPE");
            accept(node.Type);
            pop();
        }

        public void Visit(Lambda node)
        {
            write($"LAMBDA");
            accept(node.Parameter);
            accept(node.Stmt);
        }

        public void Visit(Identifier node)
        {
            write($"IDENTIFIER {node.Name}");
        }

        public void Visit(ExpressionArgument node)
        {
            write($"ARGUMENT");
            accept(node.Expression);
        }

        public void Visit(ExpressionStmt node)
        {
            write($"EXPRESSSION STATEMET");
            accept(node.RightExpression);
        }

        public void Visit(AndExpr node)
        {
            write($"AND");
            accept(node.FirstOperand);

            foreach (var item in node.Operands)
            {
                accept(item);
            }
        }

        public void Visit(OrExpr node)
        {
            write($"OR");
            accept(node.FirstOperand);

            foreach (var item in node.Operands)
            {
                accept(item);
            }
        }

        public void Visit(AdditiveExpr node)
        {
            write($"ADD");
            accept(node.FirstOperand);

            push();
            foreach (var item in node.Operands)
            {
                write($"{item.Item1}");
                accept(item.Item2);
            }
            pop();
        }

        public void Visit(MultiplicativeExpr node)
        {
            write($"MUL");
            accept(node.FirstOperand);

            push();
            foreach (var item in node.Operands)
            {
                write($"{item.Item1}");
                accept(item.Item2);
            }
            pop();
        }

        public void Visit(NegativeExpr node)
        {
            write($"NEG {node.Operator}");
            accept(node.Operand);
        }

        public void Visit(ConversionExpr node)
        {
            write($"CONVERSION");
            accept(node.OryginalExpression);

            push();
            write($"TO");
            accept(node.TypeExpression);
            pop();
        }


        public void Visit(FunctionCallExpr node)
        {
            write($"FUNCTION CALL {node.Name}");

            foreach (var item in node.Arguments)
            {
                accept(item);
            }
        }

        public void Visit(ComparativeExpr node)
        {
            write($"COMPARE");
            accept(node.FirstOperand);

            push();
            foreach (var item in node.Operands)
            {
                write($"{item.Item1}");
                accept(item.Item2);
            }
            pop();
        }

        public void Visit(PrctOfExpr node)
        {
            write($"PRCT");
            node.FirstOperand.Accept(this);

            push();
            write($"OF");
            accept(node.SecondOperand);
            pop();
        }

        public void Visit(ConstructiorCallExpr node)
        {
            write($"CONSTRUCTOR");

            push();
            write($"CONSTRUCTOR TYPE");
            accept(node.Type);
            foreach (var item in node.Arguments)
            {
                accept(item);
            }
            pop();
        }

        public void Visit(ObjectPropertyExpr node)
        {
            write($"PROPERTY {node.Property} OF");
            node.Object.Accept(this);
        }

        public void Visit(ObjectIndexExpr node)
        {
            write($"INDEX OF");
            node.Object.Accept(this);

            push();
            write($"INDEX EXPR");
            accept(node.IndexExpression);
            pop();
        }

        public void Visit(ObjectMethodExpr node)
        {
            write($"METHOD {node.Method} OF");
            accept(node.Object);

            push();
            foreach (var item in node.Arguments)
            {
                write("ARGUMENT");
                accept(item);
            }
            pop();
        }

        public void Visit(BracedExprTerm node)
        {
            write($"BRACED");
            accept(node.Expression);
        }

        public void Visit(Literal node)
        {
            write($"LITERAL");

            push();
            if (node.Type.Type == TypeEnum.TYPE)
            {
                write($"LITERAL TYPE");
                accept(node.ValueType!);
            }
            else
            {
                write($"LITERAL VALUE {(Object)node.BoolValue! ?? (Object)node.IntValue! ?? (Object)node.StringValue! ?? (Object)node.DecimalValue!}");
            }
            pop();
        }

        private static void writeIndentation(int depth)
        {
            Console.Write(Enumerable.Repeat(' ', depth).ToArray());
        }

        private void write(string text)
        {
            writeIndentation(depth);
            Console.WriteLine(text);
        }

        public void Visit(BasicType type)
        {
            write($"Type {type.Name} ({type.Type})");
        }

        public void Visit(GenericType type)
        {
            write($"Type of {type.Name} ({type.Type}) parametrised by");
            type.ParametrisingType.Accept(this);
        }

        public void Visit(TypeType type)
        {
            write($"Type of {type.Name} ({type.Type}) representing ({type.OfType}) parametrised by");
        }

        public void Visit(NoneType type)
        {
            write($"Type of EMPTY");
        }

        public void push()
        {
            depth++;
        }

        public void pop()
        {
            depth--;
        }

        public void accept(IVisitable visitable)
        {
            push();
            visitable.Accept(this);
            pop();
        }
    }
}
