namespace nai.nai;

using Expressive.Expressions;
using Expressive.Functions;
using System.Numerics;
using Expressive;

public class Formula
{
    private Expression expression;
    public Formula(string formula)
    {
        expression = new Expression(formula, ExpressiveOptions.NoCache);
        expression.RegisterFunction(new MapFunction());
    }


    public long GetPrice(NovelAIEngine engine, long step, Vector2 size) =>
        (long)expression.Evaluate<double>(new Dictionary<string, object>()
        {
            { "minAnlas", engine.minPrice },
            { "maxAnlas", engine.maxPrice },
            { "step", step },
            { "quality", ((long)(size.X * size.Y)) },
        });
}

internal class MapFunction : IFunction
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