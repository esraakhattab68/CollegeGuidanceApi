namespace CollegeGuideApi.Helpers
{

    public class MailRequest
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<FileAttachment> Attachments { get; set; }
    }
}
public class FileAttachment
{
    public byte[] File { get; set; }
    public string Name { get; set; }
    public string ContentType { get; set; }

}
