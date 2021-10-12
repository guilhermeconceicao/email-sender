namespace Email.Sender.Template
{
    public static class TemplateDefaultReplacement
    {
        public static TemplateDefaultCreator ReplaceTitle(this TemplateDefaultCreator creator, string titulo)
        {
            creator.DefineTemplate(creator.GetTemplate().Replace("@Title", titulo));
            return creator;
        }

        public static TemplateDefaultCreator ReplaceRecipient(this TemplateDefaultCreator creator, string franqueado)
        {
            creator.DefineTemplate(creator.GetTemplate().Replace("@Recipient", franqueado));
            return creator;
        }

        public static TemplateDefaultCreator ReplaceMessage(this TemplateDefaultCreator creator, string mensagem)
        {
            creator.DefineTemplate(creator.GetTemplate().Replace("@Message", mensagem));
            return creator;
        }
    }
}