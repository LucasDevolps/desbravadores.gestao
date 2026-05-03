using Desbravadores.Gestao.Application.Common.Behaviors;
using FluentValidation;

namespace Desbravadores.Gestao.UnitTests.Application;

public sealed class ValidationBehaviorTests
{
  [Fact]
  public async Task Handle_calls_next_when_there_are_no_validators()
  {
    var behavior = new ValidationBehavior<BehaviorRequest, string>([]);
    var called = false;

    var result = await behavior.Handle(
      new BehaviorRequest(""),
      _ =>
      {
        called = true;
        return Task.FromResult("ok");
      },
      CancellationToken.None);

    Assert.True(called);
    Assert.Equal("ok", result);
  }

  [Fact]
  public async Task Handle_calls_next_when_validation_passes()
  {
    var behavior = new ValidationBehavior<BehaviorRequest, string>([new BehaviorRequestValidator()]);
    var called = false;

    var result = await behavior.Handle(
      new BehaviorRequest("value"),
      _ =>
      {
        called = true;
        return Task.FromResult("ok");
      },
      CancellationToken.None);

    Assert.True(called);
    Assert.Equal("ok", result);
  }

  [Fact]
  public async Task Handle_throws_validation_exception_and_does_not_call_next_when_invalid()
  {
    var behavior = new ValidationBehavior<BehaviorRequest, string>([new BehaviorRequestValidator()]);
    var called = false;

    await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(
      new BehaviorRequest(""),
      _ =>
      {
        called = true;
        return Task.FromResult("ok");
      },
      CancellationToken.None));

    Assert.False(called);
  }

  private sealed record BehaviorRequest(string Value);

  private sealed class BehaviorRequestValidator : AbstractValidator<BehaviorRequest>
  {
    public BehaviorRequestValidator()
    {
      RuleFor(x => x.Value).NotEmpty();
    }
  }
}
