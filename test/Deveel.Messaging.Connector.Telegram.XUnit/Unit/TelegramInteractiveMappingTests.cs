//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Deveel.Messaging.Testing;

namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "TelegramInteractiveMapping")]
public class TelegramInteractiveMappingTests
{
    [Fact]
    public void Should_MapButtonContent_When_CreateInlineKeyboardMarkup()
    {
        var buttons = InteractiveContentBuilder.CreateButtons(2);

        var markup = TelegramMessageBuilder.CreateInlineKeyboardMarkup(buttons);

        TelegramInteractiveMappingAssertions.AssertMapsToInlineKeyboard(buttons, markup);
    }

    [Fact]
    public void Should_MapQuickReplyContent_When_CreateReplyKeyboardMarkup()
    {
        var quickReplies = InteractiveContentBuilder.CreateQuickReplies(3);

        var markup = TelegramMessageBuilder.CreateReplyKeyboardMarkup(quickReplies);

        TelegramInteractiveMappingAssertions.AssertMapsToReplyKeyboard(quickReplies, markup);
    }
}
