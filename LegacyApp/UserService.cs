using System;

namespace LegacyApp
{
    public class UserService
    {

        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!IsValidInput(firstName, lastName, email, dateOfBirth))
                return false;
            
            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);

            var user = CreateUser(firstName, lastName, email, dateOfBirth, client);
            CalculateCreditLimit(user);

            if (!IsCreditLimitSufficient(user))
                return false;

            UserDataAccess.AddUser(user);
            return true;
        }

        private bool IsValidInput(string firstName, string lastName, string email, DateTime dateOfBirth)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                !email.Contains("@") || !email.Contains("."))
                return false;

            int age = DateTime.Today.Year - dateOfBirth.Year;
            if (dateOfBirth > DateTime.Today.AddYears(-age)) age--;

            return age >= 21;
        }

        private User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, Client client)
        {
            return new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
        }

        private void CalculateCreditLimit(User user)
        {
            if (user.Client.Type == "VeryImportantClient")
                user.HasCreditLimit = false;
            else
            {
                var userCreditService = new UserCreditService();
                int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                user.CreditLimit = user.Client.Type == "ImportantClient" ? creditLimit * 2 : creditLimit;
                user.HasCreditLimit = true;
            }
        }

        private bool IsCreditLimitSufficient(User user)
        {
            return !user.HasCreditLimit || user.CreditLimit >= 500;
        }
    }
}
