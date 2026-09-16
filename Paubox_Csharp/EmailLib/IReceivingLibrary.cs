namespace Paubox
{
    public interface IReceivingLibrary
    {
        ReceivingDomainListResponse ListReceivingDomains();
        ReceivingDomainResponse CreateReceivingDomain(string slug = null);
        ReceivingDomainResponse GetReceivingDomain(int domainId);
        DeleteReceivingDomainResponse DeleteReceivingDomain(int domainId);
        MailboxListResponse ListMailboxes(int domainId);
        MailboxResponse CreateMailbox(int domainId, CreateMailboxRequest request);
        MailboxResponse GetMailbox(int domainId, int mailboxId);
        DeleteMailboxResponse DeleteMailbox(int domainId, int mailboxId);
        ReceivedEmailListResponse ListReceivedEmails(ListReceivedEmailsParams parameters = null);
        ReceivedEmailResponse GetReceivedEmail(string emailId);
        string DownloadAttachment(string emailId, string blobId);
    }
}
