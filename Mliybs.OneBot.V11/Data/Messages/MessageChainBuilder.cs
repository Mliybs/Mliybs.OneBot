using System;
using System.Collections.Generic;
using System.Text;

namespace Mliybs.OneBot.V11.Data.Messages
{
    public class MessageChainBuilder
    {
        private readonly MessageChain chain = new();

        public static MessageChainBuilder New => new();

        public MessageChain Build() => chain;

        public MessageChainBuilder Add(MessageBase message)
        {
            chain.Add(message);
            return this;
        }

        public MessageChainBuilder Text(string text)
        {
            chain.Add(new TextMessage()
            {
                Data = new()
                {
                    Text = text
                }
            });
            return this;
        }

        public MessageChainBuilder Image(string image)
        {
            chain.Add(new ImageMessage()
            {
                Data = new()
                {
                    File = image
                }
            });
            return this;
        }
        
        public MessageChainBuilder Face(string face)
        {
            chain.Add(new FaceMessage()
            {
                Data = new()
                {
                    Id = face
                }
            });
            return this;
        }

        public MessageChainBuilder Record(string record)
        {
            chain.Add(new RecordMessage()
            {
                Data = new()
                {
                    File = record
                }
            });
            return this;
        }

        public MessageChainBuilder Video(string video)
        {
            chain.Add(new VideoMessgae()
            {
                Data = new()
                {
                    File = video
                }
            });
            return this;
        }

        public MessageChainBuilder At(string qq)
        {
            chain.Add(new AtMessage()
            {
                Data = new()
                {
                    QQ = qq
                }
            });
            return this;
        }

        public MessageChainBuilder Rps()
        {
            chain.Add(new RpsMessage());
            return this;
        }

        public MessageChainBuilder Dice()
        {
            chain.Add(new DiceMessage());
            return this;
        }

        public MessageChainBuilder Shake()
        {
            chain.Add(new ShakeMessgae());
            return this;
        }

        public MessageChainBuilder Poke(string type, long id)
        {
            chain.Add(new PokeMessage()
            {
                Data = new()
                {
                    Type = type,
                    Id = id
                }
            });
            return this;
        }

        public MessageChainBuilder Anonymous(bool ignore)
        {
            chain.Add(new AnonymousMessage()
            {
                Data = new()
                {
                    Ignore = ignore
                }
            });
            return this;
        }

        public MessageChainBuilder Share(string url, string title, string? content = null, string? image = null)
        {
            chain.Add(new ShareMessage()
            {
                Data = new()
                {
                    Url = url,
                    Title = title,
                    Content = content,
                    Image = image
                }
            });
            return this;
        }

        public MessageChainBuilder Contact(ContactMessage.ContactType type, long id)
        {
            chain.Add(new ContactMessage()
            {
                Data = new()
                {
                    Type = type,
                    Id = id
                }
            });
            return this;
        }

        public MessageChainBuilder Location(string lat, string lon, string? title = null, string? content = null)
        {
            chain.Add(new LocationMessage()
            {
                Data = new()
                {
                    Lat = lat,
                    Lon = lon,
                    Title = title,
                    Content = content
                }
            });
            return this;
        }

        public MessageChainBuilder Music(string type, string id)
        {
            chain.Add(new MusicMessage()
            {
                Data = new()
                {
                    Type = type,
                    Id = id
                }
            });
            return this;
        }

        public MessageChainBuilder Music(string type, string url, string audio, string title)
        {
            chain.Add(new MusicMessage()
            {
                Data = new()
                {
                    Type = type,
                    Url = url,
                    Audio = audio,
                    Title = title
                }
            });
            return this;
        }

        public MessageChainBuilder Reply(int id)
        {
            chain.Add(new ReplyMessage()
            {
                Data = new()
                {
                    Id = id
                }
            });
            return this;
        }

        public MessageChainBuilder Node(long user, string nickname, MessageChain messgae)
        {
            chain.Add(new NodeMessage()
            {
                Data = new()
                {
                    UserId = user,
                    Nickname = nickname,
                    Content = messgae
                }
            });
            return this;
        }

        public MessageChainBuilder Node(int id)
        {
            chain.Add(new NodeMessage()
            {
                Data = new()
                {
                    Id = id
                }
            });
            return this;
        }

        public MessageChainBuilder Xml(string xml)
        {
            chain.Add(new XmlMessage()
            {
                Data = new()
                {
                    Data = xml
                }
            });
            return this;
        }

        public MessageChainBuilder Json(string json)
        {
            chain.Add(new JsonMessage()
            {
                Data = new()
                {
                    Data = json
                }
            });
            return this;
        }
    }
}
