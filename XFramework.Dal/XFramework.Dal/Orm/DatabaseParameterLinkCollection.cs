namespace XFramework.Dal.Orm
{
    public class DatabaseParameterLinkCollection : IDatabaseParameterLink
    {
        public DatabaseParameterRelation? Releation { get; set; }
        public IDatabaseParameterLink Next { get; set; }

        public DatabaseParameterLink Content { get; set; }

        public DatabaseParameterLinkCollection(DatabaseParameter parameter,
            DatabaseParameterRelation relation = DatabaseParameterRelation.AND)
        {
            var link = new DatabaseParameterLink()
            {
                Parameter = parameter
            };

            Releation = relation;
            Content = link;
        }

        public DatabaseParameterLinkCollection()
        {

        }

        public DatabaseParameterLink Append(DatabaseParameter parameter, DatabaseParameterRelation relation = DatabaseParameterRelation.AND)
        {
            var link = new DatabaseParameterLink()
            {
                Parameter = parameter,
                Releation = relation
            };

            Next = link;

            return link;
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
