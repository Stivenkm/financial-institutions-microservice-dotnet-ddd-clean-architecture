using FluentValidation;

namespace Intec.Banking.FinancialInstitutions.Primitives;
public class CommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> DispatchAsync<TResponse>(
        ICommand<TResponse> command, 
        CancellationToken ct = default)
    {
        var handlerType = typeof(ICommandHandler<,>)
            .MakeGenericType(command.GetType(), typeof(TResponse));
        
        var handler = _serviceProvider.GetRequiredService(handlerType);
        
        var method = handlerType.GetMethod("HandleAsync")!;
        var result = method.Invoke(handler, new object[] { command, ct });
        
        return await (Task<TResponse>)result!;
    }
}
public class QueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public QueryDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> DispatchAsync<TResponse>(
        IQuery<TResponse> query, 
        CancellationToken ct = default)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(query.GetType());
        var validator = _serviceProvider.GetService(validatorType);

        if (validator is IValidator fluentValidator)
        {
            var context = new ValidationContext<object>(query);
            var result = await fluentValidator.ValidateAsync(context, ct);

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }

        var handlerType = typeof(IQueryHandler<,>)
            .MakeGenericType(query.GetType(), typeof(TResponse));
        
        var handler = _serviceProvider.GetRequiredService(handlerType);
        
        var method = handlerType.GetMethod("HandleAsync")!;
        var response = method.Invoke(handler, new object[] { query, ct });
        
        return await (Task<TResponse>)response!;
    }
}