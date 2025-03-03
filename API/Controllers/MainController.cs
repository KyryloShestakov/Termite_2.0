using API.Application.Services;
using Microsoft.AspNetCore.Mvc;
using ModelsLib.BlockchainLib;
using RRLib.Responses;
using Utilities;
using LogLevel = Utilities.LogLevel;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MainController : ControllerBase
{
    private readonly Service _service;
    
    public MainController(Service service)
    {
          _service = service;
    }

    [HttpGet("GetAddress")]
    public async Task<IActionResult> GetAddressAndPrivateKey()
    {
        var result = await _service.GetAddressAndPrivateKey();
        return Ok(result);
    }

    [HttpPost("PostTransaction")]
    public async Task<IActionResult> PostTransaction([FromBody] TransactionModel transaction)
    {
        if (transaction == null)
        {
            return BadRequest("Transaction data is missing.");
        }
        
        await _service.PostTransaction(transaction);
        return Ok();
    }

    [HttpPost("GetBalance")]
    public async Task<IActionResult> GetBalance([FromBody] BalanceRequest address)
    {
        if (address == null) return BadRequest("Address is missing.");
        var balance = await _service.GetBalance(address.Address);
        return Ok(balance);
    }

    [HttpPost("GetTransaction")]
    public async Task<IActionResult> GetTransaction([FromBody] string transactionId)
    {
        try
        {
            if (transactionId == null) return BadRequest("TransactionId is missing.");
            Response result = await _service.GetTransaction(transactionId);
            return Ok(result);
        }
        catch (Exception e)
        {
            Logger.Log($"{e.Message}", LogLevel.Error, Source.API);
            throw;
        }
    }
    
    [HttpPost("GetTransactionsByAddress")]
    public async Task<IActionResult> GetTransactionsById([FromBody] string address)
    {
        try
        {
            if (address == null) return BadRequest("address is missing.");
            Response result = await _service.GetTransactionById(address);
            return Ok(result);
        }
        catch (Exception e)
        {
            Logger.Log($"{e.Message}", LogLevel.Error, Source.API);
            throw;
        }
    }
    
    
}

public class BalanceRequest
{
    public string Address { get; set; }
}