using Backend.Infrastructure.Persistence;

namespace Backend.Tests.Infrastructure;

public sealed class ConnectionStringNormalizerTests
{
    [Fact]
    public void Uri_Full_Pooler_Format()
    {
        var result = ConnectionStringNormalizer.Normalize(
            "postgresql://postgres.ref:pa%2Fss%4040@aws-0-us-west-2.pooler.supabase.com:5432/postgres?sslmode=require");

        Assert.Equal(
            "Host=aws-0-us-west-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.ref;Password=pa/ss@40;SSL Mode=Require",
            result);
    }

    [Fact]
    public void Uri_Without_Port_Defaults_5432()
    {
        var result = ConnectionStringNormalizer.Normalize("postgres://u:p@db.ref.supabase.co/postgres");

        Assert.Equal("Host=db.ref.supabase.co;Port=5432;Database=postgres;Username=u;Password=p;SSL Mode=Require", result);
    }

    [Fact]
    public void KeyValue_Passes_Through_Trimmed_And_Unquoted()
    {
        const string input = "  \"Host=db;Port=5432;Database=postgres;Username=u;Password=p;SSL Mode=Require\"  ";

        Assert.Equal("Host=db;Port=5432;Database=postgres;Username=u;Password=p;SSL Mode=Require",
            ConnectionStringNormalizer.Normalize(input));
    }

    [Fact]
    public void Empty_Stays_Empty_For_Fail_Fast()
    {
        Assert.Equal(string.Empty, ConnectionStringNormalizer.Normalize(null));
        Assert.Equal(string.Empty, ConnectionStringNormalizer.Normalize("   "));
    }

    [Fact]
    public void Malformed_Uri_Throws_Clear_Error()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ConnectionStringNormalizer.Normalize("postgresql://no-user-info-here"));

        Assert.Contains("key=value", ex.Message);
    }
}
