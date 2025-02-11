﻿using ApiStone.Data;
using ApiStone.Data.Dtos.Account;
using ApiStone.Data.Dtos.Deposit;
using ApiStone.Data.Dtos.Operation;
using ApiStone.Data.Dtos.Withdraw;
using ApiStone.Enuns;
using ApiStone.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using static ApiStone.Enuns.EnumStatus;

namespace ApiStone.Services
{
    public class AccountService : IAccountService
    {
        #region Properties
        private readonly AccountDbContext _context;
        private readonly IMapper _mapper;
        # endregion

        # region Constructor
        public AccountService(AccountDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        #endregion Constructor

        #region Account Methods

        #region PostAccount
        /// <summary>
        /// Method to create a new account
        /// </summary>
        /// <param name="accountPostDto"></param>
        /// <returns></returns>
        public async Task<AccountGetDto> PostAccountAsync(AccountPostDto accountPostDto)
        {
            var account = _mapper.Map<Account>(accountPostDto);
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();
            return _mapper.Map<AccountGetDto>(account);

        }

        #endregion PostAccount

        #region GetAllAccounts

        /// <summary>
        /// Method to get all accounts
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<AccountGetDto>> GetAllAccountsAsync()
        {
            var accounts = await _context.Accounts.ToListAsync();
            return _mapper.Map<IEnumerable<AccountGetDto>>(accounts);
        }

        #endregion GetAllAccounts

        #region GetAllAccount By Id

        /// <summary>
        /// Method to get account by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AccountGetDto> GetAccountAsync(int id)
        {
            Account? account = await GetAccount(id);
            return _mapper.Map<AccountGetDto>(account);
        }


        #endregion GetAllAccount By Id

        #region PutAccount By Id

        /// <summary>
        /// Method to update an account by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="accountPutDto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AccountGetDto> PutAccountAsync(int id, AccountPutDto accountPutDto)
        {
            Account? account = await GetAccount(id);
            _mapper.Map(accountPutDto, account);
            await _context.SaveChangesAsync();
            return _mapper.Map<AccountGetDto>(account);
        }

        #endregion PutAccount By Id

        #region DeleteAccount By Id

        /// <summary>
        /// Method to delete and account by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AccountGetDto> DeleteAccountAsync(int id)
        {
            Account? account = await GetAccount(id);
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return _mapper.Map<AccountGetDto>(account);
        }

        #endregion DeleteAccount By Id

        #endregion Account Methods

        #region Deposit Methods

        #region PostDeposit By Id

        /// <summary>
        /// Method to create a new deposit
        /// </summary>
        /// <param name="id"></param>
        /// <param name="depositDto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<DepositGetDto> PostDepositAsync(int id, DepositPostDto depositDto)
        {
            Account? account = await GetAccount(id);

            if (depositDto.Amount <= 0 || depositDto.Amount == null)
            {
                throw new Exception("Invalid amount");
            }

            else
            {
                account.Balance += depositDto.Amount;
                var operation = _mapper.Map<Operation>(depositDto);
                operation.AccountId = id;
                operation.Type = OperationType.Deposit;
                operation.Status = OperationStatus.Executed;
                await _context.Operations.AddAsync(operation);
                await _context.SaveChangesAsync();
                return _mapper.Map<DepositGetDto>(operation);
            }

        }

        #endregion PostDeposit By Id

        #region PostDeposit By Date

        /// <summary>
        /// Method to create a new future deposit
        /// </summary>
        /// <param name="id"></param>
        /// <param name="date"></param>
        /// <param name="depositDto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<DepositGetDto> PostDepositByDateAsync(int id, DateTime date, DepositPostDto depositDto)
        {
            Account? account = await GetAccount(id);

            if (depositDto.Amount <= 0 || depositDto.Amount == null)
            {
                throw new Exception("Invalid value");
            }
            else if (date < DateTime.Now || date == null)
            {
                throw new Exception("Invalid date");
            }

            else
            {

                var operation = _mapper.Map<Operation>(depositDto);
                operation.AccountId = id;
                operation.Type = OperationType.FutureDeposit;
                operation.Status = OperationStatus.Scheduled;
                operation.ScheduledAt = date;

                await _context.Operations.AddAsync(operation);
                await _context.SaveChangesAsync();
                return _mapper.Map<DepositGetDto>(operation);
            }
        }

        #endregion PostDeposit By Date

        #region GetDeposit By Id

        /// <summary>
        /// Method to get deposit by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<DepositGetDto> GetDepositAsync(int id)
        {
            var operation = await _context.Operations.FindAsync(id);
            if (operation == null)
            {
                throw new Exception("Operation not found");
            }
            else if (operation.Type != OperationType.Deposit)
            {
                throw new Exception("Operation is not a deposit");
            }
            return _mapper.Map<DepositGetDto>(operation);
        }

        #endregion GetDeposit By Id

        #endregion Deposit Methods

        #region Withdraw Methods

        #region PostWithdraw By Id

        /// <summary>
        /// Method to create new withdraw
        /// </summary>
        /// <param name="id"></param>
        /// <param name="withdrawDto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<WithdrawGetDto> PostWithdrawAsync(int id, WithdrawPostDto withdrawDto)
        {
            Account? account = await GetAccount(id);

            if (withdrawDto.Amount <= 0 || withdrawDto.Amount == null)
            {
                throw new Exception("Amount invalid");
            }

            else if (account.Balance < withdrawDto.Amount)
            {
                throw new Exception("Insufficient funds");
            }

            else
            {
                account.Balance -= withdrawDto.Amount;
                var operation = _mapper.Map<Operation>(withdrawDto);
                operation.AccountId = id;
                operation.Type = OperationType.Withdraw;
                operation.Status = OperationStatus.Executed;
                await _context.Operations.AddAsync(operation);
                await _context.SaveChangesAsync();
                return _mapper.Map<WithdrawGetDto>(operation);
            }

        }

        #endregion PostWithdraw By Id

        #region PostWithdraw By Date

        /// <summary>
        /// Method to create a new future withdraw
        /// </summary>
        /// <param name="id"></param>
        /// <param name="date"></param>
        /// <param name="withdrawDto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<WithdrawGetDto> PostWithdrawByDateAsync(int id, DateTime date, WithdrawPostDto withdrawDto)
        {
            Account? account = await GetAccount(id);

            if (withdrawDto.Amount <= 0 || withdrawDto.Amount == null)
            {
                throw new Exception("Amount invalid");
            }

            else if (account.Balance < withdrawDto.Amount)
            {
                throw new Exception("Insufficient funds");
            }

            else
            {
                var operation = _mapper.Map<Operation>(withdrawDto);
                operation.AccountId = id;
                operation.Type = OperationType.FutureWithdraw;
                operation.Status = OperationStatus.Scheduled;
                operation.ScheduledAt = date;
                await _context.Operations.AddAsync(operation);
                await _context.SaveChangesAsync();
                return _mapper.Map<WithdrawGetDto>(operation);
            }

        }

        #endregion PostWithdraw By Date

        #region GetWithdraw By Id

        /// <summary>
        /// Method to get withdraw by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<WithdrawGetDto> GetWithdrawAsync(int id)
        {
            var operation = await _context.Operations.FindAsync(id);
            if (operation == null)
            {
                throw new Exception("Operation not found");
            }
            else if (operation.Type != OperationType.Withdraw)
            {
                throw new Exception("Operation is not a withdraw");
            }
            return _mapper.Map<WithdrawGetDto>(operation);
        }

        #endregion GetWithdraw By Id

        #endregion Withdraw Methods

        #region Statement Methods

        #region GetAllOperations By Account Id

        /// <summary>
        /// Method to get all operations by accountId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<IEnumerable<OperationGetDto>> GetAllOperationsAsync(int id)
        {
            Account? account = await GetAccount(id);

            var operations = await _context.Operations.Where(x => x.AccountId == id).ToListAsync();
            return _mapper.Map<IEnumerable<OperationGetDto>>(operations);
        }

        #endregion GetAllOperations By Account Id

        #region GetOperations By Account Id And Date

        /// <summary>
        /// Method to get operation by accountId and future date
        /// </summary>
        /// <param name="id"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<IEnumerable<OperationGetDto>> GetOperationsByDateAsync(int id, DateTime date)
        {
            Account? account = await GetAccount(id);

            var operations = await _context.Operations.Where(x => x.AccountId == id && x.ScheduledAt == date).ToListAsync();
            return _mapper.Map<IEnumerable<OperationGetDto>>(operations);
        }

        #endregion GetOperations By Account Id And Date

        #endregion Statement Methods

        #region Balance Methods

        #region GetBalance By Id

        /// <summary>
        /// Method to get an account balance by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AccountBalanceGetDto> GetBalanceAsync(int id)
        {
            Account? account = await GetAccount(id);

            return _mapper.Map<AccountBalanceGetDto>(account);

        }

        #endregion GetBalance By Id

        #region GetBalance By Date

        /// <summary>
        /// Method to get an account balance by future date
        /// </summary>
        /// <param name="id"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AccountBalanceGetDto> GetBalanceByDateAsync(int id, DateTime date)
        {
            Account? account = await GetAccount(id);

            var operations = await _context.Operations.Where(x => x.AccountId == id).ToListAsync();
            foreach (var operation in operations)
            {
                if (operation.ScheduledAt <= date) 
                {
                    if (operation.Type == OperationType.FutureDeposit) 
                    {
                        account.Balance += operation.Amount; 
                        operation.Status = OperationStatus.Executed; 
                    }
                    else if (operation.Type == OperationType.FutureWithdraw) 
                    {
                        account.Balance -= operation.Amount; 
                        operation.Status = OperationStatus.Executed; 
                    }
                }
            }

            return _mapper.Map<AccountBalanceGetDto>(account);

        }

        #endregion GetBalance By Date

        #endregion Balance Methods


        #region GetAccount Method 

        private async Task<Account> GetAccount(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                throw new Exception("Account not found");
            }

            return account;
        }

        #endregion GetAccount Method


    }
}
