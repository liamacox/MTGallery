using System.Text.Json;
using MTGallery;

namespace MTGallery.Domain.UnitTests;

public class card_constructor
{
    [Fact]
    public void card_constructs_from_json()
    {
      const string jsonData = """
                              [
                                {
                                  "object": "card",
                                  "id": "6124a691-ae83-4d22-a177-0aee65b47064",
                                  "oracle_id": "3ba809ce-3556-415c-9fcc-291cb10a43cd",
                                  "multiverse_ids": [],
                                  "resource_id": "43CF3C2830803B34F7AF0706511BE6395EA04B5E6A16BC5B452147455AB26D83",
                                  "mtgo_id": 146387,
                                  "tcgplayer_id": 671748,
                                  "cardmarket_id": 865832,
                                  "name": "Ajani, Outland Chaperone",
                                  "lang": "en",
                                  "released_at": "2026-01-23",
                                  "uri": "https://api.scryfall.com/cards/6124a691-ae83-4d22-a177-0aee65b47064",
                                  "scryfall_uri": "https://scryfall.com/card/ecl/4/ajani-outland-chaperone?utm_source=api",
                                  "layout": "normal",
                                  "highres_image": true,
                                  "image_status": "highres_scan",
                                  "image_uris": {
                                    "small": "https://cards.scryfall.io/small/front/6/1/6124a691-ae83-4d22-a177-0aee65b47064.jpg?1767951721",
                                    "normal": "https://cards.scryfall.io/normal/front/6/1/6124a691-ae83-4d22-a177-0aee65b47064.jpg?1767951721",
                                    "large": "https://cards.scryfall.io/large/front/6/1/6124a691-ae83-4d22-a177-0aee65b47064.jpg?1767951721",
                                    "png": "https://cards.scryfall.io/png/front/6/1/6124a691-ae83-4d22-a177-0aee65b47064.png?1767951721",
                                    "art_crop": "https://cards.scryfall.io/art_crop/front/6/1/6124a691-ae83-4d22-a177-0aee65b47064.jpg?1767951721",
                                    "border_crop": "https://cards.scryfall.io/border_crop/front/6/1/6124a691-ae83-4d22-a177-0aee65b47064.jpg?1767951721"
                                  },
                                  "mana_cost": "{1}{W}{W}",
                                  "cmc": 3.0,
                                  "type_line": "Legendary Planeswalker \u2014 Ajani",
                                  "oracle_text": "\u002B1: Create a 1/1 green and white Kithkin creature token.\n\u22122: Ajani deals 4 damage to target tapped creature.\n\u22128: Look at the top X cards of your library, where X is your life total. You may put any number of nonland permanent cards with mana value 3 or less from among them onto the battlefield. Then shuffle.",
                                  "loyalty": "3",
                                  "colors": [
                                    "W"
                                  ],
                                  "color_identity": [
                                    "W"
                                  ],
                                  "keywords": [],
                                  "all_parts": [
                                    {
                                      "object": "related_card",
                                      "id": "2ed11e1b-2289-48d2-8d96-ee7e590ecfd4",
                                      "component": "token",
                                      "name": "Kithkin",
                                      "type_line": "Token Creature \u2014 Kithkin",
                                      "uri": "https://api.scryfall.com/cards/2ed11e1b-2289-48d2-8d96-ee7e590ecfd4"
                                    },
                                    {
                                      "object": "related_card",
                                      "id": "b5734a2c-0e8f-4087-97ee-51e26cfbf62d",
                                      "component": "combo_piece",
                                      "name": "Ajani, Outland Chaperone",
                                      "type_line": "Legendary Planeswalker \u2014 Ajani",
                                      "uri": "https://api.scryfall.com/cards/b5734a2c-0e8f-4087-97ee-51e26cfbf62d"
                                    }
                                  ],
                                  "legalities": {
                                    "standard": "legal",
                                    "future": "legal",
                                    "historic": "legal",
                                    "timeless": "legal",
                                    "gladiator": "legal",
                                    "pioneer": "legal",
                                    "modern": "legal",
                                    "legacy": "legal",
                                    "pauper": "not_legal",
                                    "vintage": "legal",
                                    "penny": "not_legal",
                                    "commander": "legal",
                                    "oathbreaker": "legal",
                                    "standardbrawl": "legal",
                                    "brawl": "legal",
                                    "alchemy": "legal",
                                    "paupercommander": "not_legal",
                                    "duel": "legal",
                                    "oldschool": "not_legal",
                                    "premodern": "not_legal",
                                    "predh": "not_legal"
                                  },
                                  "games": [
                                    "paper",
                                    "arena",
                                    "mtgo"
                                  ],
                                  "reserved": false,
                                  "game_changer": false,
                                  "foil": true,
                                  "nonfoil": true,
                                  "finishes": [
                                    "nonfoil",
                                    "foil"
                                  ],
                                  "oversized": false,
                                  "promo": false,
                                  "reprint": false,
                                  "variation": false,
                                  "set_id": "5d293ad8-a749-4725-bd5c-c4e1db828bd0",
                                  "set": "ecl",
                                  "set_name": "Lorwyn Eclipsed",
                                  "set_type": "expansion",
                                  "set_uri": "https://api.scryfall.com/sets/5d293ad8-a749-4725-bd5c-c4e1db828bd0",
                                  "set_search_uri": "https://api.scryfall.com/cards/search?order=set\u0026q=e%3Aecl\u0026unique=prints",
                                  "scryfall_set_uri": "https://scryfall.com/sets/ecl?utm_source=api",
                                  "rulings_uri": "https://api.scryfall.com/cards/6124a691-ae83-4d22-a177-0aee65b47064/rulings",
                                  "prints_search_uri": "https://api.scryfall.com/cards/search?order=released\u0026q=oracleid%3A3ba809ce-3556-415c-9fcc-291cb10a43cd\u0026unique=prints",
                                  "collector_number": "4",
                                  "digital": false,
                                  "rarity": "mythic",
                                  "card_back_id": "0aeebaf5-8c7d-4636-9e82-8c27447861f7",
                                  "artist": "Daren Bader",
                                  "artist_ids": [
                                    "7da1a585-c875-45e4-b322-5da9e8e1f651"
                                  ],
                                  "illustration_id": "7b995a99-974b-40bd-8d1c-10ebf8f791b1",
                                  "border_color": "black",
                                  "frame": "2015",
                                  "security_stamp": "oval",
                                  "full_art": false,
                                  "textless": false,
                                  "booster": true,
                                  "story_spotlight": false,
                                  "edhrec_rank": 14614,
                                  "prices": {
                                    "usd": "2.20",
                                    "usd_foil": "3.35",
                                    "usd_etched": null,
                                    "eur": "2.74",
                                    "eur_foil": "2.94",
                                    "tix": "3.03"
                                  },
                                  "related_uris": {
                                    "tcgplayer_infinite_articles": "https://partner.tcgplayer.com/c/4931599/1830156/21018?subId1=api\u0026trafcat=tcgplayer.com%2Fsearch%2Farticles\u0026u=https%3A%2F%2Fwww.tcgplayer.com%2Fsearch%2Farticles%3FproductLineName%3Dmagic%26q%3DAjani%252C%2BOutland%2BChaperone",
                                    "tcgplayer_infinite_decks": "https://partner.tcgplayer.com/c/4931599/1830156/21018?subId1=api\u0026trafcat=tcgplayer.com%2Fsearch%2Fdecks\u0026u=https%3A%2F%2Fwww.tcgplayer.com%2Fsearch%2Fdecks%3FproductLineName%3Dmagic%26q%3DAjani%252C%2BOutland%2BChaperone",
                                    "edhrec": "https://edhrec.com/route/?cc=Ajani%2C\u002BOutland\u002BChaperone"
                                  },
                                  "purchase_uris": {
                                    "tcgplayer": "https://partner.tcgplayer.com/c/4931599/1830156/21018?subId1=api\u0026u=https%3A%2F%2Fwww.tcgplayer.com%2Fproduct%2F671748%3Fpage%3D1",
                                    "cardmarket": "https://www.cardmarket.com/en/Magic/Products?idProduct=865832\u0026referrer=scryfall\u0026utm_campaign=card_prices\u0026utm_medium=text\u0026utm_source=scryfall",
                                    "cardhoarder": "https://www.cardhoarder.com/cards/146387?affiliate_id=scryfall\u0026ref=card-profile\u0026utm_campaign=affiliate\u0026utm_medium=card\u0026utm_source=scryfall"
                                  }
                                }
                              ]
                              """;
      var jsonArray = JsonDocument.Parse(jsonData).RootElement;
      var cardJson = jsonArray.EnumerateArray().First();

      var card = Card.BuildCardFromJson(cardJson);
      
      
      Assert.Equal("3ba809ce-3556-415c-9fcc-291cb10a43cd", card.OracleId);
      Assert.Equal("Ajani, Outland Chaperone", card.Name);
      Assert.Equal(Rarity.Mythic, card.Rarity);
      Assert.Equal("https://scryfall.com/card/ecl/4/ajani-outland-chaperone?utm_source=api", card.ScryfallUri);
      Assert.Equal("https://cards.scryfall.io/small/front/6/1/6124a691-ae83-4d22-a177-0aee65b47064.jpg?1767951721", card.ImageUri);
      Assert.Equal("6124a691-ae83-4d22-a177-0aee65b47064", card.ScryfallId);
      Assert.Equal("ecl", card.Set);
      Assert.Equal(4, card.CollectorNumber);
    }
    
    [Fact]
    public void multiface_card_constructs_from_json()
    {
      const string jsonData = """
                              [
                                  {
                                     "object":"card",
                                     "id":"b2d9d5ca-7e15-437a-bdfc-5972b42148fe",
                                     "oracle_id":"36af3c2c-49d7-46ea-ab02-c254b332448e",
                                     "multiverse_ids":[
                                        
                                     ],
                                     "mtgo_id":146407,
                                     "tcgplayer_id":656563,
                                     "cardmarket_id":862567,
                                     "name":"Eirdu, Carrier of Dawn // Isilu, Carrier of Twilight",
                                     "lang":"en",
                                     "released_at":"2026-01-23",
                                     "uri":"https://api.scryfall.com/cards/b2d9d5ca-7e15-437a-bdfc-5972b42148fe",
                                     "scryfall_uri":"https://scryfall.com/card/ecl/13/eirdu-carrier-of-dawn-isilu-carrier-of-twilight?utm_source=api",
                                     "layout":"transform",
                                     "highres_image":true,
                                     "image_status":"highres_scan",
                                     "cmc":5,
                                     "type_line":"Legendary Creature - Elemental God // Legendary Creature - Elemental God",
                                     "color_identity":[
                                        "B",
                                        "W"
                                     ],
                                     "keywords":[
                                        "Flying",
                                        "Lifelink",
                                        "Transform"
                                     ],
                                     "card_faces":[
                                        {
                                           "object":"card_face",
                                           "name":"Eirdu, Carrier of Dawn",
                                           "mana_cost":"{3}{W}{W}",
                                           "type_line":"Legendary Creature - Elemental God",
                                           "oracle_text":"Flying, lifelink\nCreature spells you cast have convoke. (Your creatures can help cast those spells. Each creature you tap while casting a creature spell pays for {1} or one mana of that creature's color.)\nAt the beginning of your first main phase, you may pay {B}. If you do, transform Eirdu.",
                                           "colors":[
                                              "W"
                                           ],
                                           "power":"5",
                                           "toughness":"5",
                                           "artist":"Lucas Graciano",
                                           "artist_id":"ce98f39c-7cdd-47e6-a520-6c50443bb4c2",
                                           "illustration_id":"3cbe36f0-9789-45cd-9e44-b9458af205f7",
                                           "image_uris":{
                                              "small":"https://cards.scryfall.io/small/front/b/2/b2d9d5ca-7e15-437a-bdfc-5972b42148fe.jpg?1759144812",
                                              "normal":"https://cards.scryfall.io/normal/front/b/2/b2d9d5ca-7e15-437a-bdfc-5972b42148fe.jpg?1759144812",
                                              "large":"https://cards.scryfall.io/large/front/b/2/b2d9d5ca-7e15-437a-bdfc-5972b42148fe.jpg?1759144812",
                                              "png":"https://cards.scryfall.io/png/front/b/2/b2d9d5ca-7e15-437a-bdfc-5972b42148fe.png?1759144812",
                                              "art_crop":"https://cards.scryfall.io/art_crop/front/b/2/b2d9d5ca-7e15-437a-bdfc-5972b42148fe.jpg?1759144812",
                                              "border_crop":"https://cards.scryfall.io/border_crop/front/b/2/b2d9d5ca-7e15-437a-bdfc-5972b42148fe.jpg?1759144812"
                                           }
                                        },
                                        {
                                           "object":"card_face",
                                           "name":"Isilu, Carrier of Twilight",
                                           "mana_cost":"",
                                           "type_line":"Legendary Creature - Elemental God",
                                           "oracle_text":"Flying, lifelink\nEach other nontoken creature you control has persist. (When it dies, if it had no -1/-1 counters on it, return it to the battlefield under its owner's control with a -1/-1 counter on it.)\nAt the beginning of your first main phase, you may pay {W}. If you do, transform Isilu.",
                                           "colors":[
                                              "B"
                                           ],
                                           "color_indicator":[
                                              "B"
                                           ],
                                           "power":"5",
                                           "toughness":"5",
                                           "artist":"Lucas Graciano",
                                           "artist_id":"ce98f39c-7cdd-47e6-a520-6c50443bb4c2",
                                           "illustration_id":"90357c65-bc08-457d-8c9e-1bc66da89ac2",
                                           "image_uris":{
                                              "small":"https://cards.scryfall.io/small/back/b/2/b2d9d5ca-7e15-437a-bdfc-5972b42148fe.jpg?1759144812",
                                              "normal":"https://cards.scryfall.io/normal/back/b/2/b2d9d5ca-7e15-437a-bdfc-5972b42148fe.jpg?1759144812",
                                              "large":"https://cards.scryfall.io/large/back/b/2/b2d9d5ca-7e15-437a-bdfc-5972b42148fe.jpg?1759144812",
                                              "png":"https://cards.scryfall.io/png/back/b/2/b2d9d5ca-7e15-437a-bdfc-5972b42148fe.png?1759144812",
                                              "art_crop":"https://cards.scryfall.io/art_crop/back/b/2/b2d9d5ca-7e15-437a-bdfc-5972b42148fe.jpg?1759144812",
                                              "border_crop":"https://cards.scryfall.io/border_crop/back/b/2/b2d9d5ca-7e15-437a-bdfc-5972b42148fe.jpg?1759144812"
                                           }
                                        }
                                     ],
                                     "legalities":{
                                        "standard":"legal",
                                        "future":"legal",
                                        "historic":"legal",
                                        "timeless":"legal",
                                        "gladiator":"legal",
                                        "pioneer":"legal",
                                        "modern":"legal",
                                        "legacy":"legal",
                                        "pauper":"not_legal",
                                        "vintage":"legal",
                                        "penny":"not_legal",
                                        "commander":"legal",
                                        "oathbreaker":"legal",
                                        "standardbrawl":"legal",
                                        "brawl":"legal",
                                        "alchemy":"legal",
                                        "paupercommander":"not_legal",
                                        "duel":"legal",
                                        "oldschool":"not_legal",
                                        "premodern":"not_legal",
                                        "predh":"not_legal"
                                     },
                                     "games":[
                                        "paper",
                                        "arena",
                                        "mtgo"
                                     ],
                                     "reserved":false,
                                     "game_changer":false,
                                     "foil":true,
                                     "nonfoil":true,
                                     "finishes":[
                                        "nonfoil",
                                        "foil"
                                     ],
                                     "oversized":false,
                                     "promo":false,
                                     "reprint":false,
                                     "variation":false,
                                     "set_id":"5d293ad8-a749-4725-bd5c-c4e1db828bd0",
                                     "set":"ecl",
                                     "set_name":"Lorwyn Eclipsed",
                                     "set_type":"expansion",
                                     "set_uri":"https://api.scryfall.com/sets/5d293ad8-a749-4725-bd5c-c4e1db828bd0",
                                     "set_search_uri":"https://api.scryfall.com/cards/search?order=set&q=e%3Aecl&unique=prints",
                                     "scryfall_set_uri":"https://scryfall.com/sets/ecl?utm_source=api",
                                     "rulings_uri":"https://api.scryfall.com/cards/b2d9d5ca-7e15-437a-bdfc-5972b42148fe/rulings",
                                     "prints_search_uri":"https://api.scryfall.com/cards/search?order=released&q=oracleid%3A36af3c2c-49d7-46ea-ab02-c254b332448e&unique=prints",
                                     "collector_number":"13",
                                     "digital":false,
                                     "rarity":"mythic",
                                     "artist":"Lucas Graciano",
                                     "artist_ids":[
                                        "ce98f39c-7cdd-47e6-a520-6c50443bb4c2"
                                     ],
                                     "border_color":"black",
                                     "frame":"2015",
                                     "frame_effects":[
                                        "legendary"
                                     ],
                                     "security_stamp":"oval",
                                     "full_art":false,
                                     "textless":false,
                                     "booster":true,
                                     "story_spotlight":false,
                                     "edhrec_rank":8067,
                                     "prices":{
                                        "usd":"5.06",
                                        "usd_foil":"6.42",
                                        "usd_etched":null,
                                        "eur":"5.45",
                                        "eur_foil":"7.26",
                                        "tix":"2.30"
                                     },
                                     "related_uris":{
                                        "tcgplayer_infinite_articles":"https://partner.tcgplayer.com/c/4931599/1830156/21018?subId1=api&trafcat=tcgplayer.com%2Fsearch%2Farticles&u=https%3A%2F%2Fwww.tcgplayer.com%2Fsearch%2Farticles%3FproductLineName%3Dmagic%26q%3DEirdu%252C%2BCarrier%2Bof%2BDawn%2B%252F%252F%2BIsilu%252C%2BCarrier%2Bof%2BTwilight",
                                        "tcgplayer_infinite_decks":"https://partner.tcgplayer.com/c/4931599/1830156/21018?subId1=api&trafcat=tcgplayer.com%2Fsearch%2Fdecks&u=https%3A%2F%2Fwww.tcgplayer.com%2Fsearch%2Fdecks%3FproductLineName%3Dmagic%26q%3DEirdu%252C%2BCarrier%2Bof%2BDawn%2B%252F%252F%2BIsilu%252C%2BCarrier%2Bof%2BTwilight",
                                        "edhrec":"https://edhrec.com/route/?cc=Eirdu%2C+Carrier+of+Dawn"
                                     },
                                     "purchase_uris":{
                                        "tcgplayer":"https://partner.tcgplayer.com/c/4931599/1830156/21018?subId1=api&u=https%3A%2F%2Fwww.tcgplayer.com%2Fproduct%2F656563%3Fpage%3D1",
                                        "cardmarket":"https://www.cardmarket.com/en/Magic/Products?idProduct=862567&referrer=scryfall&utm_campaign=card_prices&utm_medium=text&utm_source=scryfall",
                                        "cardhoarder":"https://www.cardhoarder.com/cards/146407?affiliate_id=scryfall&ref=card-profile&utm_campaign=affiliate&utm_medium=card&utm_source=scryfall"
                                     }
                                  }
                              ]
                              """;
      var jsonArray = JsonDocument.Parse(jsonData).RootElement;
      var cardJson = jsonArray.EnumerateArray().First();

      var card = Card.BuildCardFromJson(cardJson);
      
      
      Assert.Equal("36af3c2c-49d7-46ea-ab02-c254b332448e", card.OracleId);
      Assert.Equal("Eirdu, Carrier of Dawn // Isilu, Carrier of Twilight", card.Name);
      Assert.Equal(Rarity.Mythic, card.Rarity);
      Assert.Equal("https://scryfall.com/card/ecl/13/eirdu-carrier-of-dawn-isilu-carrier-of-twilight?utm_source=api", card.ScryfallUri);
      Assert.Equal("https://cards.scryfall.io/small/front/b/2/b2d9d5ca-7e15-437a-bdfc-5972b42148fe.jpg?1759144812", card.ImageUri);
      Assert.Equal("b2d9d5ca-7e15-437a-bdfc-5972b42148fe", card.ScryfallId);
      Assert.Equal("ecl", card.Set);
      Assert.Equal(13, card.CollectorNumber);
    }
}