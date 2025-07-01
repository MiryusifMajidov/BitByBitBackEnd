using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.Business.Helpers
{
    public static class EmailTemplateHelper
    {
        /// <summary>
        /// Email confirmation code HTML template
        /// </summary>
        public static string GetConfirmationCodeTemplate(string firstName, string confirmationCode)
        {
            return $@"
<!DOCTYPE html>
<html lang=""az"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Email Təsdiqi</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            margin: 0;
            padding: 0;
            background-color: #f4f4f4;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            padding: 20px;
            border-radius: 10px;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            border-radius: 10px 10px 0 0;
            margin: -20px -20px 20px -20px;
        }}
        .logo {{
            font-size: 28px;
            font-weight: bold;
            margin-bottom: 10px;
        }}
        .subtitle {{
            font-size: 16px;
            opacity: 0.9;
        }}
        .content {{
            padding: 20px 0;
        }}
        .greeting {{
            font-size: 18px;
            color: #333;
            margin-bottom: 20px;
        }}
        .code-container {{
            text-align: center;
            margin: 30px 0;
            padding: 25px;
            background: #f8f9fa;
            border-radius: 10px;
            border-left: 4px solid #667eea;
        }}
        .code {{
            font-size: 36px;
            font-weight: bold;
            color: #667eea;
            letter-spacing: 8px;
            margin: 10px 0;
            font-family: 'Courier New', monospace;
        }}
        .code-label {{
            font-size: 14px;
            color: #666;
            margin-bottom: 10px;
        }}
        .message {{
            font-size: 16px;
            color: #555;
            line-height: 1.6;
            margin: 20px 0;
        }}
        .warning {{
            background-color: #fff3cd;
            border: 1px solid #ffeaa7;
            color: #856404;
            padding: 15px;
            border-radius: 5px;
            margin: 20px 0;
            font-size: 14px;
        }}
        .footer {{
            text-align: center;
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #eee;
            color: #888;
            font-size: 14px;
        }}
        .footer-brand {{
            color: #667eea;
            font-weight: bold;
        }}
        .button {{
            display: inline-block;
            padding: 15px 30px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            margin: 20px 0;
        }}
        .social-links {{
            margin-top: 20px;
        }}
        .social-links a {{
            color: #667eea;
            text-decoration: none;
            margin: 0 10px;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div class=""logo"">🔐 BitByBit</div>
            <div class=""subtitle"">Email Təsdiq Sistemi</div>
        </div>
        
        <div class=""content"">
            <div class=""greeting"">
                Salam <strong>{firstName}</strong>,
            </div>
            
            <div class=""message"">
                BitByBit hesabınızı təsdiqləmək üçün aşağıdakı təsdiq kodunu daxil edin:
            </div>
            
            <div class=""code-container"">
                <div class=""code-label"">TƏSDİQ KODU</div>
                <div class=""code"">{confirmationCode}</div>
            </div>
            
            <div class=""warning"">
                ⚠️ <strong>Diqqət:</strong> Bu kod yalnız 24 saat müddətində keçərlidir. 
                Əgər siz bu tələbi etməmisinizsə, bu email-i nəzərə almayın.
            </div>
            
            <div class=""message"">
                Kodu daxil etdikdən sonra hesabınız aktivləşəcək və platformamızdan tam şəkildə istifadə edə biləcəksiniz.
            </div>
        </div>
        
        <div class=""footer"">
            <div>Hörmətlə, <span class=""footer-brand"">BitByBit</span> komandası</div>
            <div class=""social-links"">
                <a href=""#"">📧 Dəstək</a> | 
                <a href=""#"">🌐 Veb sayt</a> | 
                <a href=""#"">📱 Tətbiq</a>
            </div>
            <div style=""margin-top: 15px; font-size: 12px;"">
                © 2024 BitByBit. Bütün hüquqlar qorunur.
            </div>
        </div>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Welcome email HTML template
        /// </summary>
        public static string GetWelcomeTemplate(string firstName)
        {
            return $@"
<!DOCTYPE html>
<html lang=""az"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Xoş gəlmisiniz</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            margin: 0;
            padding: 0;
            background-color: #f4f4f4;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            padding: 20px;
            border-radius: 10px;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
            color: white;
            padding: 40px;
            border-radius: 10px 10px 0 0;
            margin: -20px -20px 20px -20px;
        }}
        .logo {{
            font-size: 32px;
            margin-bottom: 10px;
        }}
        .title {{
            font-size: 24px;
            font-weight: bold;
            margin-bottom: 10px;
        }}
        .subtitle {{
            font-size: 16px;
            opacity: 0.9;
        }}
        .content {{
            padding: 20px 0;
        }}
        .greeting {{
            font-size: 20px;
            color: #333;
            margin-bottom: 20px;
            text-align: center;
        }}
        .welcome-message {{
            font-size: 16px;
            color: #555;
            line-height: 1.8;
            margin: 20px 0;
            text-align: center;
        }}
        .features {{
            background: #f8f9fa;
            padding: 25px;
            border-radius: 10px;
            margin: 25px 0;
            border-left: 4px solid #11998e;
        }}
        .features h3 {{
            color: #11998e;
            margin-bottom: 15px;
        }}
        .feature-list {{
            list-style: none;
            padding: 0;
        }}
        .feature-list li {{
            padding: 8px 0;
            color: #555;
        }}
        .feature-list li:before {{
            content: '✅';
            margin-right: 10px;
        }}
        .cta-button {{
            text-align: center;
            margin: 30px 0;
        }}
        .button {{
            display: inline-block;
            padding: 15px 30px;
            background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
            color: white;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            font-size: 16px;
        }}
        .footer {{
            text-align: center;
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #eee;
            color: #888;
            font-size: 14px;
        }}
        .footer-brand {{
            color: #11998e;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div class=""logo"">🎉</div>
            <div class=""title"">Xoş gəlmisiniz!</div>
            <div class=""subtitle"">BitByBit ailəsinə qoşuldunuz</div>
        </div>
        
        <div class=""content"">
            <div class=""greeting"">
                Salam <strong>{firstName}</strong>! 👋
            </div>
            
            <div class=""welcome-message"">
                BitByBit platformasına xoş gəlmisiniz! Hesabınız uğurla yaradıldı və 
                artıq bütün funksiyalardan istifadə edə bilərsiniz.
            </div>
            
            <div class=""features"">
                <h3>🚀 Nə edə bilərsiniz:</h3>
                <ul class=""feature-list"">
                    <li>Profilinizi redaktə edə bilərsiniz</li>
                    <li>Şifrənizi istənilən vaxt dəyişə bilərsiniz</li>
                    <li>Hesab parametrlərini idarə edə bilərsiniz</li>
                    <li>Dəstək komandası ilə əlaqə saxlaya bilərsiniz</li>
                    <li>Bütün premium funksiyalardan istifadə edə bilərsiniz</li>
                </ul>
            </div>
            
            <div class=""cta-button"">
                <a href=""#"" class=""button"">🔐 İndi Daxil Olun</a>
            </div>
            
            <div class=""welcome-message"">
                Sualınız varsa, bizə yazmaqdan çəkinməyin. BitByBit komandası 
                həmişə sizə kömək etməyə hazırdır! 💬
            </div>
        </div>
        
        <div class=""footer"">
            <div>Hörmətlə, <span class=""footer-brand"">BitByBit</span> komandası 💙</div>
            <div style=""margin-top: 15px; font-size: 12px;"">
                © 2024 BitByBit. Bütün hüquqlar qorunur.
            </div>
        </div>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Password reset HTML template
        /// </summary>
        public static string GetPasswordResetTemplate(string firstName, string resetCode)
        {
            return $@"
<!DOCTYPE html>
<html lang=""az"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Şifrə Sıfırlama</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            margin: 0;
            padding: 0;
            background-color: #f4f4f4;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            padding: 20px;
            border-radius: 10px;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            background: linear-gradient(135deg, #ff6b6b 0%, #ee5a52 100%);
            color: white;
            padding: 30px;
            border-radius: 10px 10px 0 0;
            margin: -20px -20px 20px -20px;
        }}
        .logo {{
            font-size: 28px;
            font-weight: bold;
            margin-bottom: 10px;
        }}
        .subtitle {{
            font-size: 16px;
            opacity: 0.9;
        }}
        .content {{
            padding: 20px 0;
        }}
        .greeting {{
            font-size: 18px;
            color: #333;
            margin-bottom: 20px;
        }}
        .code-container {{
            text-align: center;
            margin: 30px 0;
            padding: 25px;
            background: #fff5f5;
            border-radius: 10px;
            border-left: 4px solid #ff6b6b;
        }}
        .code {{
            font-size: 42px;
            font-weight: bold;
            color: #ff6b6b;
            letter-spacing: 12px;
            margin: 10px 0;
            font-family: 'Courier New', monospace;
        }}
        .code-label {{
            font-size: 14px;
            color: #666;
            margin-bottom: 10px;
        }}
        .message {{
            font-size: 16px;
            color: #555;
            line-height: 1.6;
            margin: 20px 0;
        }}
        .warning {{
            background-color: #fff3cd;
            border: 1px solid #ffeaa7;
            color: #856404;
            padding: 15px;
            border-radius: 5px;
            margin: 20px 0;
            font-size: 14px;
        }}
        .security-notice {{
            background-color: #f8d7da;
            border: 1px solid #f5c6cb;
            color: #721c24;
            padding: 15px;
            border-radius: 5px;
            margin: 20px 0;
            font-size: 14px;
        }}
        .footer {{
            text-align: center;
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #eee;
            color: #888;
            font-size: 14px;
        }}
        .footer-brand {{
            color: #ff6b6b;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div class=""logo"">🔑 BitByBit</div>
            <div class=""subtitle"">Şifrə Sıfırlama</div>
        </div>
        
        <div class=""content"">
            <div class=""greeting"">
                Salam <strong>{firstName}</strong>,
            </div>
            
            <div class=""message"">
                Şifrənizi sıfırlamaq üçün tələb aldıq. Aşağıdakı sıfırlama kodunu istifadə edin:
            </div>
            
            <div class=""code-container"">
                <div class=""code-label"">SİFIRLAMA KODU</div>
                <div class=""code"">{resetCode}</div>
            </div>
            
            <div class=""warning"">
                ⏰ <strong>Diqqət:</strong> Bu kod yalnız 30 dəqiqə müddətində keçərlidir.
            </div>
            
            <div class=""security-notice"">
                🛡️ <strong>Təhlükəsizlik xəbərdarlığı:</strong> Əgər siz bu tələbi etməmisinizsə, 
                hesabınızın təhlükəsizliyi üçün dərhal bizimlə əlaqə saxlayın və şifrənizi dəyişin.
            </div>
            
            <div class=""message"">
                Kodu daxil etdikdən sonra yeni şifrə təyin edə biləcəksiniz.
            </div>
        </div>
        
        <div class=""footer"">
            <div>Hörmətlə, <span class=""footer-brand"">BitByBit</span> Təhlükəsizlik Komandası</div>
            <div style=""margin-top: 15px; font-size: 12px;"">
                © 2024 BitByBit. Bütün hüquqlar qorunur.
            </div>
        </div>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Generic HTML template wrapper
        /// </summary>
        public static string GetGenericTemplate(string title, string content, string primaryColor = "#667eea")
        {
            return $@"
<!DOCTYPE html>
<html lang=""az"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{title}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            margin: 0;
            padding: 0;
            background-color: #f4f4f4;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            padding: 20px;
            border-radius: 10px;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            background: {primaryColor};
            color: white;
            padding: 30px;
            border-radius: 10px 10px 0 0;
            margin: -20px -20px 20px -20px;
        }}
        .content {{
            padding: 20px 0;
            color: #555;
            font-size: 16px;
        }}
        .footer {{
            text-align: center;
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #eee;
            color: #888;
            font-size: 14px;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>BitByBit</h1>
            <p>{title}</p>
        </div>
        <div class=""content"">
            {content}
        </div>
        <div class=""footer"">
            <p>Hörmətlə, <strong>BitByBit</strong> komandası</p>
            <p style=""font-size: 12px;"">© 2024 BitByBit. Bütün hüquqlar qorunur.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}