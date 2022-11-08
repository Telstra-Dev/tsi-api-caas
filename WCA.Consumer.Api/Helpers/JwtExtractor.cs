using System.Linq;
using System.IdentityModel.Tokens.Jwt;

public static class JwtExtractor {
    public static string ExtractField(string token, string field) {
        var stream = token;
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(stream);
        var tokenS = jsonToken as JwtSecurityToken;
        return tokenS.Claims.First(claim => claim.Type == field).Value;
    }
}
