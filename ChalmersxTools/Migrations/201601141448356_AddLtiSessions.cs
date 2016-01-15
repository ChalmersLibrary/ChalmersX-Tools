namespace ChalmersxTools.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLtiSessions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LtiSessions",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        ConsumerKey = c.String(maxLength: 64),
                        UserId = c.String(maxLength: 64),
                        CourseOrg = c.String(maxLength: 64),
                        CourseId = c.String(maxLength: 64),
                        CourseRun = c.String(maxLength: 64),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.ConsumerKey, t.UserId, t.CourseOrg, t.CourseId, t.CourseRun }, unique: true, name: "IX_StudentWithToolOnCourseRun");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.LtiSessions", "IX_StudentWithToolOnCourseRun");
            DropTable("dbo.LtiSessions");
        }
    }
}
