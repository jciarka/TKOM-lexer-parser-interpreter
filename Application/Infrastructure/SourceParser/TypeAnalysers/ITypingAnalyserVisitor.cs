using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Presenters
{
    public interface ITypingAnalyseVisitor
    {
        TypeBase? Visit(ProgramRoot node);
        TypeBase? Visit(FunctionDecl functionDecl);

        TypeBase? Visit(BlockStmt blockStmt);
        TypeBase? Visit(IdentifierAssignmentStatement identifierAssignmentStatement);
        TypeBase? Visit(PropertyAssignmentStatement propertyAssignmentStatement);
        TypeBase? Visit(IndexAssignmentStatement indexAssignmentStatement);
        TypeBase? Visit(DeclarationStmt declarationStmt);
        TypeBase? Visit(ReturnStmt returnStmt);
        TypeBase? Visit(WhileStmt whileStmt);
        TypeBase? Visit(IfStmt ifStmt);
        TypeBase? Visit(ForeachStmt foreachStmt);
        TypeBase? Visit(FinancialToStmt financialToStmt);
        TypeBase? Visit(FinancialFromStmt financialFromStmt);

        TypeBase? Visit(Parameter parameter);
        TypeBase? Visit(Lambda lambda);
        TypeBase? Visit(Identifier identifier);
        TypeBase? Visit(ExpressionArgument expressionArgument);

        TypeBase? Visit(ExpressionStmt expressionStmt);
        TypeBase? Visit(OrExpr orExpr);
        TypeBase? Visit(AdditiveExpr additiveExpr);
        TypeBase? Visit(MultiplicativeExpr multiplicativeExpr);
        TypeBase? Visit(NegativeExpr negativeExpr);
        TypeBase? Visit(ConversionExpr conversionExpr);
        TypeBase? Visit(AndExpr andExpr);
        TypeBase? Visit(FunctionCallExpr functionCallExpr);
        TypeBase? Visit(ComparativeExpr comparativeExpr);
        TypeBase? Visit(PrctOfExpr prctOfExpr);
        TypeBase? Visit(ConstructiorCallExpr constructiorCallExpr);

        TypeBase? Visit(ObjectPropertyExpr objectPropertyExpr);
        TypeBase? Visit(ObjectIndexExpr objectIndexExpr);
        TypeBase? Visit(ObjectMethodExpr objectMethodExpr);
        TypeBase? Visit(BracedExprTerm bracedExprTerm);

        TypeBase? Visit(Literal literal);

        TypeBase? Visit(BasicType literal);
        TypeBase? Visit(GenericType literal);
        TypeBase? Visit(TypeType literal);

    }
}
