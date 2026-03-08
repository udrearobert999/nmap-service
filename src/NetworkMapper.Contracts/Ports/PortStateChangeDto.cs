namespace NetworkMapper.Contracts.Ports;

public record PortStateChangeDto(
    int Port,
    string Protocol,
    string OldState,
    string NewState);