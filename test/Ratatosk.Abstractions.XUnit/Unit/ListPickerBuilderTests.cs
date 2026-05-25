namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "ListPickerBuilder")]
public class ListPickerBuilderTests {
	[Fact]
	public void Should_BuildListPicker_When_AddItemWithArgs() {
		var list = new ListPickerBuilder()
			.AddItem("Pizza", "Delicious cheese pizza")
			.Build();

		Assert.Single(list.Items);
		Assert.Equal("Pizza", list.Items[0].Title);
		Assert.Equal("Delicious cheese pizza", list.Items[0].Description);
	}

	[Fact]
	public void Should_BuildListPicker_When_AddItemWithConfigure() {
		var list = new ListPickerBuilder()
			.AddItem(item => item
				.WithTitle("Burger")
				.WithDescription("Juicy beef burger"))
			.Build();

		Assert.Single(list.Items);
		Assert.Equal("Burger", list.Items[0].Title);
		Assert.Equal("Juicy beef burger", list.Items[0].Description);
	}

	[Fact]
	public void Should_SetStyle_When_WithStyleCalled() {
		var list = new ListPickerBuilder()
			.WithStyle(ListPickerStyle.Compact)
			.AddItem("Item")
			.Build();

		Assert.Equal(ListPickerStyle.Compact, list.Style);
	}

	[Fact]
	public void Should_DefaultStyle_When_NotSpecified() {
		var builder = new ListPickerBuilder();
		Assert.Equal(ListPickerStyle.Inlined, builder.Style);

		var list = builder.AddItem("Item").Build();
		Assert.Equal(ListPickerStyle.Inlined, list.Style);
	}

	[Fact]
	public void Should_SetStyleViaProperty_When_StylePropertySet() {
		var builder = new ListPickerBuilder { Style = ListPickerStyle.Large };
		Assert.Equal(ListPickerStyle.Large, builder.Style);
	}

	[Fact]
	public void Should_SetTitle_When_TitlePropertySet() {
		var list = new ListPickerBuilder {
			Title = "Menu",
			Subtitle = "Choose"
		}.AddItem("Item").Build();

		Assert.Equal("Menu", list.Title);
		Assert.Equal("Choose", list.Subtitle);
	}

	[Fact]
	public void Should_AddMultipleItems_When_AddItemCalledMultipleTimes() {
		var list = new ListPickerBuilder()
			.AddItem("A")
			.AddItem("B")
			.AddItem("C")
			.Build();

		Assert.Equal(3, list.Items.Count);
	}

	[Fact]
	public void Should_ThrowArgumentNullException_When_AddItemWithNullConfigure() {
		var builder = new ListPickerBuilder();

		Assert.Throws<ArgumentNullException>(() => builder.AddItem((Action<ListPickerItemBuilder>)null!));
	}

	[Fact]
	public void Should_ProduceListPickerContent_When_BuildCalled() {
		var result = new ListPickerBuilder()
			.AddItem("Item")
			.Build();

		Assert.IsType<ListPickerContent>(result);
		Assert.Equal(MessageContentType.ListPicker, result.ContentType);
	}

	[Fact]
	public void Should_AddItemWithAllArgs_When_AddItemCalled() {
		var list = new ListPickerBuilder()
			.AddItem("Salad", "Fresh", "https://img.url", "ORDER_SALAD")
			.Build();

		Assert.Equal("Salad", list.Items[0].Title);
		Assert.Equal("Fresh", list.Items[0].Description);
		Assert.Equal("https://img.url", list.Items[0].ImageUrl);
		Assert.Equal("ORDER_SALAD", list.Items[0].Payload);
	}
}

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "ListPickerItemBuilder")]
public class ListPickerItemBuilderTests {
	[Fact]
	public void Should_SetProperties_When_FluentMethodsCalled() {
		var item = new ListPickerItemBuilder()
			.WithTitle("Item")
			.WithDescription("Desc")
			.Build();

		Assert.Equal("Item", item.Title);
		Assert.Equal("Desc", item.Description);
	}

	[Fact]
	public void Should_BuildWithDefaults_When_NoValuesSet() {
		var item = new ListPickerItemBuilder().Build();

		Assert.Equal(string.Empty, item.Title);
		Assert.Null(item.Description);
		Assert.Null(item.ImageUrl);
		Assert.Null(item.Payload);
	}

	[Fact]
	public void Should_SetImageUrl_When_WithImageUrlCalled() {
		var item = new ListPickerItemBuilder()
			.WithImageUrl("https://img.url")
			.Build();

		Assert.Equal("https://img.url", item.ImageUrl);
	}

	[Fact]
	public void Should_SetPayload_When_WithPayloadCalled() {
		var item = new ListPickerItemBuilder()
			.WithPayload("PAYLOAD")
			.Build();

		Assert.Equal("PAYLOAD", item.Payload);
	}

	[Fact]
	public void Should_SetAllProperties_When_AllFluentMethodsCalled() {
		var item = new ListPickerItemBuilder()
			.WithTitle("Item")
			.WithDescription("Desc")
			.WithImageUrl("https://img.url")
			.WithPayload("PAYLOAD")
			.Build();

		Assert.Equal("Item", item.Title);
		Assert.Equal("Desc", item.Description);
		Assert.Equal("https://img.url", item.ImageUrl);
		Assert.Equal("PAYLOAD", item.Payload);
	}

	[Fact]
	public void Should_ProduceListPickerItem_When_BuildCalled() {
		var result = new ListPickerItemBuilder()
			.WithTitle("Test")
			.Build();

		Assert.IsType<ListPickerItem>(result);
	}
}
