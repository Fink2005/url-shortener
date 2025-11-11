using System;
using System.Threading.Tasks;
using MailService.Domain.Entities;

namespace MailService.Application.Abstractions;

public interface IMailSender
{
    Task SendMailAsync(MailRequest request);
}
