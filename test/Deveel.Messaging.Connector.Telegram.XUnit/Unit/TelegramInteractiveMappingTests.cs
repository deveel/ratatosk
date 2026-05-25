//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Deveel.Messaging.Testing;
using Telegram.Bot.Types.ReplyMarkups;

namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "TelegramInteractiveMapping")]
public class TelegramInteractiveMappingTests
{
    private static Type _builderType = typeof(TelegramService).Assembly.GetTypes().First(t => t.Name == "TelegramMessageBuilder");

    private static object? InvokeStatic(string methodName, object[] args)
    {
        try
        {
            var method = _builderType.GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            return method!.Invoke(null, args);
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            throw ex.InnerException!;
        }
    }

    private static InlineKeyboardMarkup CreateInlineKeyboardMarkup(IEnumerable<IButtonContent> buttons)
        => (InlineKeyboardMarkup)InvokeStatic("CreateInlineKeyboardMarkup", new object[] { buttons })!;

    private static ReplyKeyboardMarkup CreateReplyKeyboardMarkup(IEnumerable<IQuickReplyContent> quickReplies)
        => (ReplyKeyboardMarkup)InvokeStatic("CreateReplyKeyboardMarkup", new object[] { quickReplies })!;

    [Fact]
    public void Should_MapButtonContent_When_CreateInlineKeyboardMarkup()
    {
        var buttons = InteractiveContentBuilder.CreateButtons(2);

        var markup = CreateInlineKeyboardMarkup(buttons);

        TelegramInteractiveMappingAssertions.AssertMapsToInlineKeyboard(buttons, markup);
    }

    [Fact]
    public void Should_Throw_When_CreateInlineKeyboardWithPhoneNumberButton()
    {
        var button = new ButtonContent("Call", ButtonType.PhoneNumber, "+1234567890");

        Assert.Throws<NotSupportedException>(() =>
            CreateInlineKeyboardMarkup(new[] { button }));
    }

    [Fact]
    public void Should_Throw_When_CreateInlineKeyboardWithUrlButtonWithoutValue()
    {
        var button = new ButtonContent("Visit", ButtonType.Url, null);

        Assert.Throws<ArgumentException>(() =>
            CreateInlineKeyboardMarkup(new[] { button }));
    }

    [Fact]
    public void Should_Throw_When_CreateInlineKeyboardWithUrlButtonWithEmptyValue()
    {
        var button = new ButtonContent("Visit", ButtonType.Url, "");

        Assert.Throws<ArgumentException>(() =>
            CreateInlineKeyboardMarkup(new[] { button }));
    }

    [Fact]
    public void Should_MapQuickReplyContent_When_CreateReplyKeyboardMarkup()
    {
        var quickReplies = InteractiveContentBuilder.CreateQuickReplies(3);

        var markup = CreateReplyKeyboardMarkup(quickReplies);

        TelegramInteractiveMappingAssertions.AssertMapsToReplyKeyboard(quickReplies, markup);
    }
}
