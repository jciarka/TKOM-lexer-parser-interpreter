using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Presenters
{
    public interface IPresenterVisitor
    {
        void Visit(ProgramRoot node, int depth = 0);
        void Visit(FunctionDecl functionDecl, int v);

        void Visit(BlockStmt blockStmt, int v);
        void Visit(IdentifierAssignmentStatement identifierAssignmentStatement, int v);
        void Visit(PropertyAssignmentStatement propertyAssignmentStatement, int v);
        void Visit(TypeType typeType, int v);
        void Visit(IndexAssignmentStatement indexAssignmentStatement, int v);
        void Visit(DeclarationStmt declarationStmt, int v);
        void Visit(ReturnStmt returnStmt, int v);
        void Visit(WhileStmt whileStmt, int v);
        void Visit(IfStmt ifStmt, int v);
        void Visit(ForeachStmt foreachStmt, int v);
        void Visit(FinancialToStmt financialToStmt, int v);
        void Visit(FinancialFromStmt financialFromStmt, int v);

        void Visit(Parameter parameter, int v);
        void Visit(Lambda lambda, int v);
        void Visit(Identifier identifier, int v);
        void Visit(ExpressionArgument expressionArgument, int v);

        void Visit(ExpressionStmt expressionStmt, int v);
        void Visit(OrExpr orExpr, int v);
        void Visit(AdditiveExpr additiveExpr, int v);
        void Visit(MultiplicativeExpr multiplicativeExpr, int v);
        void Visit(NegativeExpr negativeExpr, int v);
        void Visit(ConversionExpr conversionExpr, int v);
        void Visit(AndExpr andExpr, int v);
        void Visit(FunctionCallExpr functionCallExpr, int v);
        void Visit(ComparativeExpr comparativeExpr, int v);
        void Visit(PrctOfExpr prctOfExpr, int v);
        void Visit(ConstructiorCallExpr constructiorCallExpr, int v);

        void Visit(ObjectPropertyExpr objectPropertyExpr, int v);
        void Visit(ObjectIndexExpr objectIndexExpr, int v);
        void Visit(ObjectMethodExpr objectMethodExpr, int v);
        void Visit(BracedExprTerm bracedExprTerm, int v);

        void Visit(Literal literal, int v);

        void Visit(NoneType noneType, int v);
        void Visit(BasicType literal, int v);
        void Visit(GenericType literal, int v);

    }
}
