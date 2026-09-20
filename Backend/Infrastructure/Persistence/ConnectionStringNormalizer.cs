namespace Backend.Infrastructure.Persistence;

/// <summary>Accepts Supabase-style URIs (<c>postgresql://user:pass@host:port/db</c>)
/// by converting them to Npgsql key=value format. Key=value strings pass
/// through untouched (Npgsql itself validates them).</summary>
public static class ConnectionStringNormalizer
{
    private const string Hint = "Use key=value format (Host=...;Port=...;Database=...;Username=...;Password=...;SSL Mode=Require) or a postgresql://user:password@host:port/database URI.";

    public static string Normalize(string? value)
    {
        var trimmed = (value ?? string.Empty).Trim().Trim('"', '\'').Trim();
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        if (trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
        {
            return FromUri(trimmed);
        }

        return trimmed;
    }

    private static string FromUri(string uri)
    {
        try
        {
            var withoutScheme = uri[(uri.IndexOf("://", StringComparison.Ordinal) + 3)..];
            var at = withoutScheme.LastIndexOf('@');
            if (at <= 0)
            {
                throw new InvalidOperationException("Missing user info (user:password@). " + Hint);
            }

            var userInfo = withoutScheme[..at];
            var hostPart = withoutScheme[(at + 1)..];
            var colon = userInfo.IndexOf(':');
            if (colon <= 0)
            {
                throw new InvalidOperationException("Missing user info (user:password@). " + Hint);
            }

            var username = Uri.UnescapeDataString(userInfo[..colon]);
            var password = Uri.UnescapeDataString(userInfo[(colon + 1)..]);

            var pathStart = hostPart.IndexOf('/');
            var queryStart = hostPart.IndexOf('?');
            var hostPort = pathStart < 0 ? hostPart : hostPart[..pathStart];
            string host;
            var port = 5432;
            if (hostPort.StartsWith('['))
            {
                var end = hostPort.IndexOf(']');
                if (end < 0)
                {
                    throw new InvalidOperationException("Malformed IPv6 host. " + Hint);
                }

                host = hostPort[..(end + 1)];
                var rest = hostPort[(end + 1)..];
                if (rest.StartsWith(':') && !int.TryParse(rest[1..], out port))
                {
                    throw new InvalidOperationException("Malformed port. " + Hint);
                }
            }
            else
            {
                var parts = hostPort.Split(':');
                host = parts[0];
                if (parts.Length > 2 || string.IsNullOrWhiteSpace(host))
                {
                    throw new InvalidOperationException("Malformed host. " + Hint);
                }

                if (parts.Length == 2 && !int.TryParse(parts[1], out port))
                {
                    throw new InvalidOperationException("Malformed port. " + Hint);
                }
            }

            var database = "postgres";
            var sslMode = "Require";
            if (pathStart >= 0)
            {
                var pathAndQuery = hostPart[pathStart..];
                var queryIndex = pathAndQuery.IndexOf('?');
                var path = queryIndex < 0 ? pathAndQuery : pathAndQuery[..queryIndex];
                if (path.Length > 1)
                {
                    database = Uri.UnescapeDataString(path.TrimStart('/'));
                }

                if (queryIndex >= 0)
                {
                    foreach (var pair in pathAndQuery[(queryIndex + 1)..].Split('&', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var kv = pair.Split('=', 2);
                        if (kv.Length == 2 && kv[0].Trim().Equals("sslmode", StringComparison.OrdinalIgnoreCase))
                        {
                            sslMode = MapSslMode(kv[1].Trim());
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(database))
            {
                throw new InvalidOperationException("Missing username or database. " + Hint);
            }

            return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode={sslMode}";
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Could not parse the connection string. Use key=value format (Host=...;Port=...;Database=...;Username=...;Password=...;SSL Mode=Require) or a postgresql://user:password@host:port/database URI.", ex);
        }
    }

    private static string MapSslMode(string value) => value.ToLowerInvariant() switch
    {
        "disable" => "Disable",
        "allow" => "Allow",
        "prefer" => "Prefer",
        "require" => "Require",
        "verify-ca" or "verifyca" => "VerifyCA",
        "verify-full" or "verifyfull" => "VerifyFull",
        _ => "Require"
    };
}
