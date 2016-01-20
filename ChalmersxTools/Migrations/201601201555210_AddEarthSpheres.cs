namespace ChalmersxTools.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEarthSpheres : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EarthSpheresImagesSubmissions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 64),
                        CourseOrg = c.String(maxLength: 64),
                        CourseId = c.String(maxLength: 64),
                        CourseRun = c.String(maxLength: 64),
                        Sphere1Url = c.String(),
                        Sphere1Name = c.String(),
                        Sphere2Url = c.String(),
                        Sphere2Name = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.UserId, t.CourseOrg, t.CourseId, t.CourseRun }, unique: true, name: "IX_UserInCourseRun");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.EarthSpheresImagesSubmissions", "IX_UserInCourseRun");
            DropTable("dbo.EarthSpheresImagesSubmissions");
        }
    }
}
