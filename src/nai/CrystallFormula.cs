namespace nai;

using Expressive.Expressions;
using Expressive.Functions;
using System.Numerics;
using Expressive;
using RandomGen;
using System;


public abstract class Formula
{
    protected void UseFunctions(Expression expression)
    {
        expression.RegisterFunction(new MapFunction());
        expression.RegisterFunction(new RangeFunction());
        expression.RegisterFunction(new PowFunction());
        expression.RegisterFunction(new RoundFunction());
    }
}

public class CrystallFormula : Formula
{
    private readonly Expression expression;
    public CrystallFormula(string formula)
    {
        expression = new Expression(formula, ExpressiveOptions.NoCache);
        UseFunctions(expression);
    }


    public long GetPrice(NovelAIEngine engine, long step, Vector2 size) =>
        expression.Evaluate<long>(new Dictionary<string, object>()
        {
            { "minAnlas", engine.minPrice },
            { "maxAnlas", engine.maxPrice },
            { "step", step },
            { "quality", ((long)(size.X * size.Y)) },
        });
}

public class SeedFormula : Formula
{
    private readonly Expression expression;
    public SeedFormula(string formula)
    {
        expression = new Expression(formula, ExpressiveOptions.NoCache);
        UseFunctions(expression);
    }

    public long GetSeed() => expression.Evaluate<long>();
}

public class RangeFunction : IFunction
{
    public object Evaluate(IExpression[] parameters, Context context)
    {
        long getLong(int index)
        {
            var p = parameters[index];

            var o = p.Evaluate(Variables);

            if (o is float or double)
                return (long)o;


            return Convert.ToInt64(o);
        }

        return Gen.Random.Numbers.Longs(getLong(0), getLong(1))();
    }

    public IDictionary<string, object> Variables { get; set; } = new Dictionary<string, object>();
    public string Name => "l";
}

internal class PowFunction : IFunction
{
    public object Evaluate(IExpression[] parameters, Context context)
    {
        long getLong(int index)
        {
            var p = parameters[index];

            var o = p.Evaluate(Variables);

            if (o is float f)
                return (long)float.Round(f);
            if (o is double d)
                return (long)double.Round(d);


            return Convert.ToInt64(o);
        }

        return (long)Math.Pow(getLong(0), getLong(1));
    }

    public IDictionary<string, object> Variables { get; set; } = new Dictionary<string, object>();
    public string Name => "pow";
}

internal class RoundFunction : IFunction
{
    public object Evaluate(IExpression[] parameters, Context context)
    {
        var number = parameters[0].Evaluate(Variables);

        if (number is float f)
            return (long)float.Round(f);
        if (number is double d)
            return (long)double.Round(d);

        if (number is long l)
            return l;
        return Convert.ToInt64(number);
    }

    public IDictionary<string, object> Variables { get; set; } = new Dictionary<string, object>();
    public string Name => "round";
}

internal class MapFunction : IFunction
{
    public object Evaluate(IExpression[] parameters, Context context)
    {
        long getLong(int index)
        {
            var p = parameters[index];

            var o = p.Evaluate(Variables);

            if (o is float f)
                return (long)float.Round(f);
            if (o is double d)
                return (long)double.Round(d);

            return Convert.ToInt64(o);
        }


        return MapFn(
            getLong(0),
            getLong(1),
            getLong(2),
            getLong(3),
            getLong(4)
        );
    }

    private long MapFn(long x, long in_min, long in_max, long out_min, long out_max) 
        => (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;

    public IDictionary<string, object> Variables { get; set; } = new Dictionary<string, object>();
    public string Name => "map";
}