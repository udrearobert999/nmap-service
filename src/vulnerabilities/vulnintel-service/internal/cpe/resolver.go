package cpe

import (
	"fmt"
	"regexp"
	"strings"

	"github.com/udrearobert999/vulnintel-service/internal/domain"
)

type vendorProduct struct {
	vendor  string
	product string
}

var dictionary = map[string]vendorProduct{
	"openssh":             {"openbsd", "openssh"},
	"apache httpd":        {"apache", "http_server"},
	"apache":              {"apache", "http_server"},
	"nginx":               {"nginx", "nginx"},
	"postgresql db":       {"postgresql", "postgresql"},
	"postgresql":          {"postgresql", "postgresql"},
	"mysql":               {"mysql", "mysql"},
	"mariadb":             {"mariadb", "mariadb"},
	"microsoft iis httpd": {"microsoft", "internet_information_services"},
	"vsftpd":              {"vsftpd", "vsftpd"},
	"proftpd":             {"proftpd", "proftpd"},
	"pure-ftpd":           {"pureftpd", "pure-ftpd"},
	"isc bind":            {"isc", "bind"},
	"dropbear sshd":       {"dropbear_ssh_project", "dropbear_ssh"},
	"exim smtpd":          {"exim", "exim"},
	"postfix smtpd":       {"postfix", "postfix"},
	"openssl":             {"openssl", "openssl"},
	"samba smbd":          {"samba", "samba"},
	"redis":               {"redis", "redis"},
	"mongodb":             {"mongodb", "mongodb"},
	"elasticsearch":       {"elastic", "elasticsearch"},
	"tomcat":              {"apache", "tomcat"},
	"apache tomcat":       {"apache", "tomcat"},
	"jetty":               {"eclipse", "jetty"},
	"lighttpd":            {"lighttpd", "lighttpd"},
}

var versionPattern = regexp.MustCompile(`[0-9]+(\.[0-9]+)*([a-z][0-9a-z]*)?`)

type Resolver struct{}

func NewResolver() *Resolver {
	return &Resolver{}
}

func (r *Resolver) Resolve(product, version string) domain.ResolvedCPE {
	normalizedProduct := strings.ToLower(strings.TrimSpace(product))
	normalizedVersion := normalizeVersion(version)

	if normalizedProduct == "" {
		return domain.ResolvedCPE{URI: "", Confidence: 0}
	}

	if vp, ok := dictionary[normalizedProduct]; ok {
		return build(vp.vendor, vp.product, normalizedVersion, dictionaryConfidence(normalizedVersion))
	}

	fallbackToken := fallbackProductToken(normalizedProduct)
	return build(fallbackToken, fallbackToken, normalizedVersion, fallbackConfidence(normalizedVersion))
}

func build(vendor, product, version string, confidence float64) domain.ResolvedCPE {
	cpeVersion := version
	if cpeVersion == "" {
		cpeVersion = "*"
	}

	uri := fmt.Sprintf("cpe:2.3:a:%s:%s:%s:*:*:*:*:*:*:*", vendor, product, cpeVersion)
	return domain.ResolvedCPE{URI: uri, Confidence: confidence}
}

func normalizeVersion(version string) string {
	trimmed := strings.TrimSpace(version)
	if trimmed == "" {
		return ""
	}

	return versionPattern.FindString(trimmed)
}

func fallbackProductToken(normalizedProduct string) string {
	fields := strings.Fields(normalizedProduct)
	if len(fields) == 0 {
		return normalizedProduct
	}

	token := fields[0]
	token = strings.ReplaceAll(token, "/", "_")
	token = strings.ReplaceAll(token, " ", "_")
	return token
}

func dictionaryConfidence(version string) float64 {
	if version != "" {
		return 0.9
	}
	return 0.6
}

func fallbackConfidence(version string) float64 {
	if version != "" {
		return 0.4
	}
	return 0.2
}
