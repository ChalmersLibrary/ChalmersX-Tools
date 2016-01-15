namespace ChalmersxTools.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewBeginning : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StudentPresentations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 64),
                        CourseOrg = c.String(maxLength: 64),
                        CourseId = c.String(maxLength: 64),
                        CourseRun = c.String(maxLength: 64),
                        Name = c.String(),
                        LocationLat = c.Double(nullable: false),
                        LocationLong = c.Double(nullable: false),
                        Introduction = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.UserId, t.CourseOrg, t.CourseId, t.CourseRun }, unique: true, name: "IX_UserInCourseRun");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.StudentPresentations", "IX_UserInCourseRun");
            DropTable("dbo.StudentPresentations");
        }
    }
}
