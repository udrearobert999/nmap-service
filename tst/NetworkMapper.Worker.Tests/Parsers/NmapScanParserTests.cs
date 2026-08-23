using NetworkMapper.Application.Worker.Parsers;

namespace NetworkMapper.Worker.Tests.Parsers;

public class NmapScanParserTests
{
    private readonly NmapScanParser _sut = new();

    [Fact]
    public void Parse_WithServiceVersionDetected_MapsProductAndVersion()
    {
        var scanId = Guid.NewGuid();
        const string xml = """
            <nmaprun>
              <host>
                <ports>
                  <port protocol="tcp" portid="22">
                    <state state="open"/>
                    <service name="ssh" product="OpenSSH" version="8.9p1 Ubuntu 3ubuntu0.6" extrainfo="Ubuntu Linux; protocol 2.0" method="probed" conf="10"/>
                  </port>
                </ports>
              </host>
            </nmaprun>
            """;

        var results = _sut.Parse(xml, scanId);

        var result = Assert.Single(results);
        Assert.Equal(scanId, result.ScanId);
        Assert.Equal(22, result.Port);
        Assert.Equal("tcp", result.Protocol);
        Assert.Equal("open", result.State);
        Assert.Equal("ssh", result.Service);
        Assert.Equal("OpenSSH", result.Product);
        Assert.Equal("8.9p1 Ubuntu 3ubuntu0.6", result.Version);
    }

    [Fact]
    public void Parse_WithoutServiceVersionDetected_LeavesProductAndVersionNull()
    {
        var scanId = Guid.NewGuid();
        const string xml = """
            <nmaprun>
              <host>
                <ports>
                  <port protocol="tcp" portid="8080">
                    <state state="open"/>
                    <service name="http-proxy" method="table" conf="3"/>
                  </port>
                </ports>
              </host>
            </nmaprun>
            """;

        var results = _sut.Parse(xml, scanId);

        var result = Assert.Single(results);
        Assert.Equal("http-proxy", result.Service);
        Assert.Null(result.Product);
        Assert.Null(result.Version);
    }

    [Fact]
    public void Parse_WithEmptyProductAndVersionAttributes_NormalizesToNull()
    {
        var scanId = Guid.NewGuid();
        const string xml = """
            <nmaprun>
              <host>
                <ports>
                  <port protocol="tcp" portid="443">
                    <state state="open"/>
                    <service name="https" product="" version="" method="table" conf="3"/>
                  </port>
                </ports>
              </host>
            </nmaprun>
            """;

        var results = _sut.Parse(xml, scanId);

        var result = Assert.Single(results);
        Assert.Null(result.Product);
        Assert.Null(result.Version);
    }
}
