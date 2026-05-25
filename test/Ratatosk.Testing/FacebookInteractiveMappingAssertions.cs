using Xunit;

using Ratatosk;

namespace Ratatosk.Testing;

public static class FacebookInteractiveMappingAssertions {
	public static void AssertMapsToQuickReply(IQuickReplyContent source, FacebookQuickReply target) {
		Assert.NotNull(target);
		Assert.Equal(source.Title, target.Title);
		Assert.Equal(source.Payload ?? source.Title, target.Payload);
		Assert.Equal(source.ImageUrl, target.ImageUrl);
	}

	public static void AssertMapsToButton(IButtonContent source, FacebookMessage target) {
		Assert.NotNull(target);
		Assert.NotNull(target.Template);
		Assert.Equal("button", target.Template.TemplateType);
		Assert.True(target.Template.Payload.ContainsKey("template_type"));
		Assert.Equal("button", target.Template.Payload["template_type"]);
		Assert.True(target.Template.Payload.ContainsKey("text"));
		Assert.Equal(source.Text, target.Template.Payload["text"]);
		Assert.True(target.Template.Payload.ContainsKey("buttons"));
	}

	public static void AssertMapsToCarousel(ICarouselContent source, FacebookMessage target) {
		Assert.NotNull(target);
		Assert.NotNull(target.Template);
		Assert.Equal("generic", target.Template.TemplateType);
		Assert.True(target.Template.Payload.ContainsKey("elements"));
		Assert.Equal(source.Cards.Count, ((System.Collections.IList)target.Template.Payload["elements"]).Count);
	}

	public static void AssertMapsToListPicker(IListPickerContent source, FacebookMessage target) {
		Assert.NotNull(target);
		Assert.NotNull(target.Template);
		Assert.Equal("list", target.Template.TemplateType);
		Assert.True(target.Template.Payload.ContainsKey("elements"));
		Assert.Equal(source.Items.Count, ((System.Collections.IList)target.Template.Payload["elements"]).Count);
		var expectedStyle = source.Style switch {
			ListPickerStyle.Inlined => "compact",
			ListPickerStyle.Compact => "compact",
			ListPickerStyle.Large => "large",
			_ => "large"
		};
		Assert.True(target.Template.Payload.ContainsKey("top_element_style"));
		Assert.Equal(expectedStyle, target.Template.Payload["top_element_style"]);
	}
}
