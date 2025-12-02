using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogEcommerce.Migrations
{
    /// <inheritdoc />
    public partial class AddRelatedProductToBlogPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RelatedProductId",
                table: "BlogPosts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_RelatedProductId",
                table: "BlogPosts",
                column: "RelatedProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPosts_Products_RelatedProductId",
                table: "BlogPosts",
                column: "RelatedProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogPosts_Products_RelatedProductId",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_RelatedProductId",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "RelatedProductId",
                table: "BlogPosts");
        }
    }
}
