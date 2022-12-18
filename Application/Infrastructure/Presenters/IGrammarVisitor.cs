using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Presenters
{
    public interface IVisitor
    {
        void Visit(ProgramRoot node);
        void Visit(FunctionDecl functionDecl);

        void Visit(BlockStmt blockStmt);
        void Visit(IdentifierAssignmentStatement identifierAssignmentStatement);
        void Visit(PropertyAssignmentStatement propertyAssignmentStatement);
        void Visit(TypeType typeType);
        void Visit(IndexAssignmentStatement indexAssignmentStatement);
        void Visit(DeclarationStmt declarationStmt);
        void Visit(ReturnStmt returnStmt);
        void Visit(WhileStmt whileStmt);
        void Visit(IfStmt ifStmt);
        void Visit(ForeachStmt foreachStmt);
        void Visit(FinancialToStmt financialToStmt);
        void Visit(FinancialFromStmt financialFromStmt);

        void Visit(Parameter parameter);
        void Visit(Lambda lambda);
        void Visit(Identifier identifier);
        void Visit(ExpressionArgument expressionArgument);

        void Visit(ExpressionStmt expressionStmt);
        void Visit(OrExpr orExpr);
        void Visit(AdditiveExpr additiveExpr);
        void Visit(MultiplicativeExpr multiplicativeExpr);
        void Visit(NegativeExpr negativeExpr);
        void Visit(ConversionExpr conversionExpr);
        void Visit(AndExpr andExpr);
        void Visit(FunctionCallExpr functionCallExpr);
        void Visit(ComparativeExpr comparativeExpr);
        void Visit(PrctOfExpr prctOfExpr);
        void Visit(ConstructiorCallExpr constructiorCallExpr);

        void Visit(ObjectPropertyExpr objectPropertyExpr);
        void Visit(ObjectIndexExpr objectIndexExpr);
        void Visit(ObjectMethodExpr objectMethodExpr);
        void Visit(BracedExprTerm bracedExprTerm);

        void Visit(Literal literal);

        void Visit(NoneType noneType);
        void Visit(BasicType literal);
        void Visit(GenericType literal);
    }
}
