using System;

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
        byte[] DownloadAttachmentBytes(string emailId, string attachmentId);

        [Obsolete("Returns the attachment as text, which corrupts binary files. Use DownloadAttachmentBytes(emailId, attachmentId) with ReceivedAttachment.Id; blob ids are no longer accepted.")]
        string DownloadAttachment(string emailId, string blobId);
    }
}
