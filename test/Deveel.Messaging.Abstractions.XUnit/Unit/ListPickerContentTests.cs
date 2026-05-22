namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "ListPickerContent")]
public class ListPickerContentTests {
	[Fact]
	public void Should_SetProperties_When_ListPickerContentConstructor() {
		var items = new List<IListPickerItem> {
			new ListPickerItem("Item 1", "Desc 1"),
			new ListPickerItem("Item 2", "Desc 2")
		};
		var content = new ListPickerContent("Title", "Subtitle", items, ListPickerStyle.Compact);

		Assert.Equal("Title", content.Title);
		Assert.Equal("Subtitle", content.Subtitle);
		Assert.Equal(2, content.Items.Count);
		Assert.Equal(ListPickerStyle.Compact, content.Style);
		Assert.Equal(MessageContentType.ListPicker, content.ContentType);
	}

	[Fact]
	public void Should_AddItem_When_AddItemCalled() {
		var content = new ListPickerContent();
		content.AddItem(new ListPickerItem("Item 1"));

		Assert.Single(content.Items);
	}

	[Fact]
	public void Should_RemoveItem_When_RemoveItemCalled() {
		var item = new ListPickerItem("Item");
		var content = new ListPickerContent(items: new[] { item });

		var result = content.RemoveItem(item);

		Assert.True(result);
		Assert.Empty(content.Items);
	}

	[Fact]
	public void Should_ClearItems_When_ClearItemsCalled() {
		var content = new ListPickerContent(items: new[] {
			new ListPickerItem("Item 1"),
			new ListPickerItem("Item 2")
		});
		content.ClearItems();

		Assert.Empty(content.Items);
	}

	[Fact]
	public void Should_CopyProperties_When_ListPickerContentCopyConstructor() {
		var source = new ListPickerContent("Src Title", "Src Sub",
			new[] { new ListPickerItem("Item", "Desc") }, ListPickerStyle.Large);
		var copy = new ListPickerContent(source);

		Assert.Equal(source.Title, copy.Title);
		Assert.Equal(source.Subtitle, copy.Subtitle);
		Assert.Equal(source.Items.Count, copy.Items.Count);
		Assert.Equal(source.Style, copy.Style);
	}

	[Fact]
	public void Should_DefaultStyle_When_InlinedSpecified() {
		var content = new ListPickerContent(style: ListPickerStyle.Inlined);
		Assert.Equal(ListPickerStyle.Inlined, content.Style);
	}

	[Fact]
	public void Should_BeInteractive_When_IInteractiveContentInterface() {
		IInteractiveContent content = new ListPickerContent();
		Assert.Equal(MessageContentType.ListPicker, content.ContentType);
	}
}

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "ListPickerItem")]
public class ListPickerItemTests {
	[Fact]
	public void Should_SetProperties_When_ListPickerItemConstructor() {
		var item = new ListPickerItem("Title", "Description", "https://img.url", "PAYLOAD");

		Assert.Equal("Title", item.Title);
		Assert.Equal("Description", item.Description);
		Assert.Equal("https://img.url", item.ImageUrl);
		Assert.Equal("PAYLOAD", item.Payload);
	}

	[Fact]
	public void Should_SetTitleOnly_When_ListPickerItemConstructorMinimal() {
		var item = new ListPickerItem("Title");

		Assert.Equal("Title", item.Title);
		Assert.Null(item.Description);
		Assert.Null(item.ImageUrl);
		Assert.Null(item.Payload);
	}

	[Fact]
	public void Should_CopyProperties_When_ListPickerItemCopyConstructor() {
		var source = new ListPickerItem("Src", "Desc", "img", "p");
		var copy = new ListPickerItem(source);

		Assert.Equal(source.Title, copy.Title);
		Assert.Equal(source.Description, copy.Description);
		Assert.Equal(source.ImageUrl, copy.ImageUrl);
		Assert.Equal(source.Payload, copy.Payload);
	}

	[Fact]
	public void Should_UpdateValues_When_ListPickerItemPropertySetters() {
		var item = new ListPickerItem("Initial");
		item.Title = "Updated";
		item.Description = "New desc";
		item.ImageUrl = "new.img";
		item.Payload = "NEW_PAYLOAD";

		Assert.Equal("Updated", item.Title);
		Assert.Equal("New desc", item.Description);
		Assert.Equal("new.img", item.ImageUrl);
		Assert.Equal("NEW_PAYLOAD", item.Payload);
	}
}
