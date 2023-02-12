using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HH_API
{
    public class EmployersResponse
    {
        [JsonPropertyName("items")]
        public Item[] Items { get; set; }

        [JsonPropertyName("found")]
        public long Found { get; set; }

        [JsonPropertyName("pages")]
        public long Pages { get; set; }

        [JsonPropertyName("per_page")]
        public long PerPage { get; set; }

        [JsonPropertyName("page")]
        public long Page { get; set; }
    }

    public class Item
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public Uri Url { get; set; }

        [JsonPropertyName("alternate_url")]
        public Uri AlternateUrl { get; set; }

        [JsonPropertyName("logo_urls")]
        public LogoUrls LogoUrls { get; set; }

        [JsonPropertyName("vacancies_url")]
        public Uri VacanciesUrl { get; set; }

        [JsonPropertyName("open_vacancies")]
        public long OpenVacancies { get; set; }
    }

    public class LogoUrls
    {
        [JsonPropertyName("90")]
        public Uri The90 { get; set; }

        [JsonPropertyName("240")]
        public Uri The240 { get; set; }

        [JsonPropertyName("original")]
        public Uri Original { get; set; }
    }

    public partial class HH_EmployersBadRespond
    {
        [JsonPropertyName("request_id")]
        public string RequestId { get; set; }

        [JsonPropertyName("errors")]
        public Error[] Errors { get; set; }

        public override string ToString()
        {
            string temp = $"Запрос {RequestId} вызвал следующие ошибки:";
            temp += string.Join("\n Причина\\Тип\\Значние:", Errors.Select(it => (it.Reason + "\\" + it.Type + "\\" + it.Value)));
            return temp;
        }
    }

    public partial class Error
    {
        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }


    public partial class EmplyerInfo
    {
        [JsonPropertyName("alternate_url")]
        public Uri AlternateUrl { get; set; }

        [JsonPropertyName("applicant_services")]
        public ApplicantServices ApplicantServices { get; set; }

        [JsonPropertyName("area")]
        public Area Area { get; set; }

        [JsonPropertyName("branded_description")]
        public string BrandedDescription { get; set; }

        [JsonPropertyName("branding")]
        public Branding Branding { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("industries")]
        public Industry[] Industries { get; set; }

        [JsonPropertyName("insider_interviews")]
        public InsiderInterview[] InsiderInterviews { get; set; }

        [JsonPropertyName("logo_urls")]
        public LogoUrls LogoUrls { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("open_vacancies")]
        public long OpenVacancies { get; set; }

        [JsonPropertyName("relations")]
        public object[] Relations { get; set; }

        [JsonPropertyName("site_url")]
        public Uri SiteUrl { get; set; }

        [JsonPropertyName("trusted")]
        public bool Trusted { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("vacancies_url")]
        public Uri VacanciesUrl { get; set; }

        public override string ToString()
        {
            string temp = $"Кратка иформация о компании {Name}:";
            temp += "\nId - " + Id + ",";
            temp += "\nСайт - " + SiteUrl + ",";
            temp += "\nКоличество отрытых вакансий - " + OpenVacancies.ToString() + ",";
            temp += "\nОписание - " + Description;
            return temp;
        }

    }

    public partial class ApplicantServices
    {
        [JsonPropertyName("target_employer")]
        public TargetEmployer TargetEmployer { get; set; }
    }

    public partial class TargetEmployer
    {
        [JsonPropertyName("count")]
        public long Count { get; set; }
    }

    public partial class Area
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public Uri Url { get; set; }
    }

    public partial class Branding
    {
        [JsonPropertyName("template_code")]
        public string TemplateCode { get; set; }

        [JsonPropertyName("template_version_id")]
        public long TemplateVersionId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public partial class Industry
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public partial class InsiderInterview
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("url")]
        public Uri Url { get; set; }
    }

    public partial class HH_ErrorNoSuchEmployer
    {
        [JsonPropertyName("request_id")]
        public string RequestId { get; set; }

        [JsonPropertyName("errors")]
        public ErrorEmp[] Errors { get; set; }

        public override string ToString()
        {
            string temp = $"Запрос {RequestId} вызвал следующие ошибки:";
            temp += string.Join("\n Тип:", Errors.Select(it => (it.Type)));
            return temp;
        }

    }

    public partial class ErrorEmp
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
