﻿using System.Net.Mail;
using System.Net;
using HtmlAgilityPack;
using SportNotifier.Asv.Models;

namespace SportNotifier.Asv;

public class Application
{
    internal Application()
    {
        DotNetEnv.Env.TraversePath().Load();
    }

    private EmailDataFormat GetData()
    {
        var html = @"https://www.basketball-reference.com/boxscores/";
        HtmlWeb web = new HtmlWeb();
        var htmlDoc = web.Load(html);
        EmailDataFormat data = new EmailDataFormat();
        HtmlNodeCollection element = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"content\"]/div[2]/h2");
        if(element != null)
            data.TotalGames = element[0].InnerText.ToString();
        element = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"content\"]/div[1]/span");
        if(element != null)
            data.Date = element[0].InnerText.ToString();
        var parentDiv = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='game_summaries']");
        if (parentDiv != null)
        {
            int childDivCount = parentDiv.SelectNodes(".//div")?.Count ?? 0;
            data.games = new List<GameDataFormat> { };
            for (int i = 0; i < childDivCount; i++)
            {
                GameDataFormat gameDataFormat = new GameDataFormat();
                element = htmlDoc.DocumentNode.SelectNodes($"//*[@id=\"content\"]/div[3]/div[{i+1}]/table[1]/tbody/tr[1]/td[1]/a");
                gameDataFormat.Team1 = element[0].InnerText.ToString();
                element = htmlDoc.DocumentNode.SelectNodes($"//*[@id=\"content\"]/div[3]/div[{i+1}]/table[1]/tbody/tr[1]/td[2]");
                gameDataFormat.Team1TotalPoints = element[0].InnerText.ToString();
                element = htmlDoc.DocumentNode.SelectNodes($"//*[@id=\"content\"]/div[3]/div[{i+1}]/table[1]/tbody/tr[2]/td[1]/a");
                gameDataFormat.Team2 = element[0].InnerText.ToString();
                element = htmlDoc.DocumentNode.SelectNodes($"//*[@id=\"content\"]/div[3]/div[{i+1}]/table[1]/tbody/tr[2]/td[2]");
                gameDataFormat.Team2TotalPoints = element[0].InnerText.ToString();
                element = htmlDoc.DocumentNode.SelectNodes($"//*[@id=\"content\"]/div[3]/div[{i+1}]/table[3]/tbody/tr[1]/td[2]/a");
                gameDataFormat.PTS_Player_Name = element[0].InnerText.ToString();
                element = htmlDoc.DocumentNode.SelectNodes($"//*[@id=\"content\"]/div[3]/div[{i+1}]/table[3]/tbody/tr[1]/td[3]");
                gameDataFormat.PTS_Player_Points = element[0].InnerText.ToString();
                element = htmlDoc.DocumentNode.SelectNodes($"//*[@id=\"content\"]/div[3]/div[{i+1}]/table[3]/tbody/tr[2]/td[2]/a");
                gameDataFormat.TRB_Player_Name = element[0].InnerText.ToString();
                element = htmlDoc.DocumentNode.SelectNodes($"//*[@id=\"content\"]/div[3]/div[{i+1}]/table[3]/tbody/tr[2]/td[3]");
                gameDataFormat.TRB_Player_Points = element[0].InnerText.ToString();
                data.games.Add(gameDataFormat);
            }
        }
        return data;
    }

    private void SendEmail(string toEmail, string? subject, string? body)
    {
        var fromAddress = new MailAddress(DotNetEnv.Env.GetString("EmailId"), DotNetEnv.Env.GetString("Name"));
        var toAddress = new MailAddress(toEmail);
        string fromPassword = DotNetEnv.Env.GetString("AppPassword");
        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
        };
        using (var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body
        })
        {
            smtp.Send(message);
        }
    }

    public void StartApp()
    {
        EmailDataFormat data = GetData(); 
        string toEmail = DotNetEnv.Env.GetString("RecipientEmail");
        string subject = $"Game Summary Report - {data.Date}";
        string body = GenerateEmailBody(data);

        SendEmail(toEmail, subject, body);
    }

    private string GenerateEmailBody(EmailDataFormat data)
    {
        var body = new System.Text.StringBuilder();

        body.AppendLine($"Total Games: {data.TotalGames}");
        body.AppendLine($"Date: {data.Date}");
        body.AppendLine("---------------------------------------");
        if (data.games != null)
        {
            foreach (var game in data.games)
            {
                body.AppendLine("Game Details:");
                body.AppendLine($"Team 1: {game.Team1} | Points: {game.Team1TotalPoints}");
                body.AppendLine($"Team 2: {game.Team2} | Points: {game.Team2TotalPoints}");
                body.AppendLine($"Top Scorer: {game.PTS_Player_Name} | Points: {game.PTS_Player_Points}");
                body.AppendLine($"Top Rebounder: {game.TRB_Player_Name} | Rebounds: {game.TRB_Player_Points}");
                body.AppendLine();
            }
        }
        return body.ToString();
    }

}