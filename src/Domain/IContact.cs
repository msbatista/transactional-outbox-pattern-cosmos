namespace Domain;

public interface IContact
{
    void SetName(string firstName, string lastName);
    void SetDescription(string description);
    void SetEmail(string email);
    void SetCompany(string companyName, string street, string houseNumber, string postalCode, string city, string country);
    void SetDeleted();
}
