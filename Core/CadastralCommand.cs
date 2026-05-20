using AdventureTools.Security.Cryptography;
using System.Linq;
using System.Text;
using Terraria.ModLoader;

namespace AdventureTools.Core;

public enum CadastralPermission
{
    None,
    View,
    Edit,
    Admin,
}
public sealed class CadastralCommand : LocalizedCommand
{
    private static byte[] Password;
    public override string Command { get; } = "cadsys";
    public override CommandType Type { get; } = CommandType.Console | CommandType.World;

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        if (args.Length == 0)
        {
            caller.Reply(this.GetLocalizedValue("NoArgs"), Error);
            return;
        }
        switch (args[0])
        {
            case "setpass":
                if (args.Length < 2)
                {
                    caller.Reply(this.GetLocalizedValue("NoPassProvided"), Error);
                }
                else
                {
                    var pass = Encoding.UTF8.GetBytes(args[1]);
                    if (pass.Length > 1024)
                    {
                        caller.Reply(this.GetLocalizedValue("PassTooLong"), Error);
                        return;
                    }
                    using var argon = new Argon2id(pass);
                    Password = argon.GetBytes(128);
                }
                break;
            case "login":
                if (caller.Player.whoAmI == 255)
                {
                    caller.Reply(this.GetLocalizedValue("AlreadyLogged"), Error);
                }
                if (args.Length < 2)
                {
                    caller.Reply(this.GetLocalizedValue("NoPassProvided"), Error);
                }
                else
                {
                    var pass = Encoding.UTF8.GetBytes(args[1]);
                    if (pass.Length > 1024)
                    {
                        caller.Reply(this.GetLocalizedValue("PassTooLong"), Error);
                        return;
                    }
                    using var argon = new Argon2id(pass);
                    if (!Password.SequenceEqual(argon.GetBytes(128)))
                    {
                        caller.Reply(this.GetLocalizedValue("WrongPass"), Error);
                        return;
                    }
                    
                }
                break;
            case "logout":
                break;
            case "get":

                break;
        }
    }
}
