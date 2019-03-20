namespace XFramework.Dal.Orm
{
    public class DatabaseParameterLink : IDatabaseParameterLink
    {
        public DatabaseParameterRelation? Releation { get; set; }
        public DatabaseParameter Parameter { get; set; }
        public IDatabaseParameterLink Next { get; set; }

        public DatabaseParameterLink(DatabaseParameter parameter)
        {
            this.Parameter = parameter;
        }

        public DatabaseParameterLink()
        {

        }

        public DatabaseParameterLink Append(DatabaseParameter parameter, 
            DatabaseParameterRelation relation = DatabaseParameterRelation.AND)
        {
            var link = new DatabaseParameterLink()
            {
                Parameter = parameter,
                Releation = relation
            };

            Next = link;

            return link;
        }

        public DatabaseParameterLink AppendCollection(DatabaseParameter parameter, 
            DatabaseParameterRelation relation = DatabaseParameterRelation.AND)
        {
            var link = new DatabaseParameterLinkCollection(parameter, relation);
            Next = link;

            return link.Content;
        }


        public DatabaseParameterLinkCollection AppendCollection(DatabaseParameterRelation relation = DatabaseParameterRelation.AND)
        {
            var link = new DatabaseParameterLinkCollection()
            {
                Releation = relation
            };

            Next = link;

            return link;
        }
    }
}
