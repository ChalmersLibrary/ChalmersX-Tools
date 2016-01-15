namespace ChalmersxTools.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PresentationInsteadOfIntroduction : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StudentPresentations", "Presentation", c => c.String());
            DropColumn("dbo.StudentPresentations", "Introduction");
        }
        
        public override void Down()
        {
            AddColumn("dbo.StudentPresentations", "Introduction", c => c.String());
            DropColumn("dbo.StudentPresentations", "Presentation");
        }
    }
}
