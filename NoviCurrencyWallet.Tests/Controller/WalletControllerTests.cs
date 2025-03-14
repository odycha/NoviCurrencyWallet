using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NoviCurrencyWallet.API.Controllers;
using NoviCurrencyWallet.Core.Contracts;
using NoviCurrencyWallet.Core.Models.Wallet;
using NoviCurrencyWallet.Core.Models.Wallet.Enums;

namespace NoviCurrencyWallet.Tests.Controller;

public class WalletControllerTests
{
	private readonly IWalletsRepository _walletsRepository;

	//We moq the dependecies of the controller
	public WalletControllerTests()
	{
		_walletsRepository = A.Fake<IWalletsRepository>();
	}


	[Fact]
	public async Task WalletController_GetWalletBalance_ReturnsOKWithoutConversion()
	{
		//Arrange
		var walletId = 1L;
		var walletBalanceDto = new GetWalletBalanceDto
		{
			Id = walletId,
			Balance = 150.50m,
			Currency = "EUR"
		};

		A.CallTo(() => _walletsRepository.GetAsync<GetWalletBalanceDto>(walletId))
			.Returns(walletBalanceDto);

		var controller = new WalletController(_walletsRepository);

		//Act
		var actionResult = await controller.GetWalletBalance(walletId);

		//Assert
		actionResult.Result.Should().BeOfType<OkObjectResult>()
			.Which.StatusCode.Should().Be(200);

		actionResult.Result.Should().BeOfType<OkObjectResult>()
			.Which.Value.Should().BeEquivalentTo(walletBalanceDto);

		A.CallTo(() => _walletsRepository.GetAsync<GetWalletBalanceDto>(walletId))
			.MustHaveHappenedOnceExactly();

	}

	[Fact]
	public async Task WalletController_GetWalletBalance_ReturnsOKWithConversion()
	{
		//Arrange
		var walletId = 1L;
		string currency = "USD";
		var walletBalanceDto = new GetWalletBalanceDto
		{
			Id = walletId,
			Balance = 150.50m,
			Currency = "EUR"
		};

		A.CallTo(() => _walletsRepository.GetAsync(walletId, currency))
			.Returns(walletBalanceDto);

		var controller = new WalletController(_walletsRepository);

		//Act
		var actionResult = await controller.GetWalletBalance(walletId, currency);

		//Assert
		actionResult.Result.Should().BeOfType<OkObjectResult>()
		.Which.Value.Should().BeEquivalentTo(walletBalanceDto);

		actionResult.Result.Should().BeOfType<OkObjectResult>()
			.Which.StatusCode.Should().Be(200);

		A.CallTo(() => _walletsRepository.GetAsync(walletId, currency))
			.MustHaveHappenedOnceExactly();
	}


	[Fact]
	public async Task WalletController_PostWallet_ReturnsOK()
	{
		//Arrange
		var createWalletDto = new CreateWalletDto
		{
			Balance = 100.00m,
			Currency = "EUR"
		};

		var createdWalletDto = new GetWalletBalanceDto
		{
			Id = 1L,
			Balance = createWalletDto.Balance,
			Currency = createWalletDto.Currency
		};

		A.CallTo(() => _walletsRepository.CreateWalletAsync(createWalletDto))
			.Returns(createdWalletDto);

		var controller = new WalletController(_walletsRepository);

		//Act
		var actionResult = await controller.PostWallet(createWalletDto);

		//Assert
		actionResult.Result.Should().BeOfType<CreatedAtActionResult>()
		.Which.Value.Should().BeEquivalentTo(createdWalletDto);

		actionResult.Result.Should().BeOfType<CreatedAtActionResult>()
			.Which.StatusCode.Should().Be(201);

		A.CallTo(() => _walletsRepository.CreateWalletAsync(createWalletDto))
			.MustHaveHappenedOnceExactly();

	}


	[Fact]
	public async Task WalletController_AdjustWalletBalance_ReturnsOk()
	{
		// Arrange
		var walletId = 1L;
		var updateWalletBalanceDto = new UpdateWalletBalanceDto
		{
			Id = walletId,
			Amount = 100.00m,
			Currency = "EUR",
			Strategy = AdjustmentStrategy.AddFundsStrategy
		};

		A.CallTo(() => _walletsRepository.AdjustBalance(updateWalletBalanceDto))
			.Returns(Task.CompletedTask);

		var controller = new WalletController(_walletsRepository);

		// Act
		var actionResult = await controller.AdjustWalletBalance(walletId, updateWalletBalanceDto);

		// Assert
		actionResult.Should().BeOfType<OkResult>()
			.Which.StatusCode.Should().Be(200);

		A.CallTo(() => _walletsRepository.AdjustBalance(updateWalletBalanceDto))
			.MustHaveHappenedOnceExactly();
	}


	[Fact]
	public async Task AdjustWalletBalance_WhenWalletIdMismatch_ReturnsBadRequest()
	{
		// Arrange
		var walletId = 1L;
		var updateWalletBalanceDto = new UpdateWalletBalanceDto
		{
			Id = 2L,  // different ID
			Amount = 50,
			Currency = "USD",
			Strategy = AdjustmentStrategy.AddFundsStrategy
		};

		var controller = new WalletController(_walletsRepository);

		// Act
		var actionResult = await controller.AdjustWalletBalance(walletId, updateWalletBalanceDto);

		// Assert
		actionResult.Should().BeOfType<BadRequestObjectResult>()
			.Which.Value.Should().Be("Invalid Record Id");

		actionResult.Should().BeOfType<BadRequestObjectResult>()
			.Which.StatusCode.Should().Be(400);

		// Ensure AdjustBalance repository method was never called
		A.CallTo(() => _walletsRepository.AdjustBalance(A<UpdateWalletBalanceDto>.Ignored))
			.MustNotHaveHappened();
	}
}