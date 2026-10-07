namespace event_tsika_back.Services;

public record PaymentResult(bool Success, string Reference);

/// <summary>
/// Point d'intégration du paiement. Seule l'implémentation simulée existe : MVola, Orange Money
/// et Airtel Money nécessitent un contrat marchand (voir docs/DECISIONS.md, ADR-004).
/// </summary>
public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(Guid userId, int amountMga);
    Task RefundAsync(string reference);
}

public class SimulatedPaymentGateway : IPaymentGateway
{
    public Task<PaymentResult> ChargeAsync(Guid userId, int amountMga) =>
        Task.FromResult(new PaymentResult(true, $"SIM-{Guid.NewGuid():N}"));

    public Task RefundAsync(string reference) => Task.CompletedTask;
}
