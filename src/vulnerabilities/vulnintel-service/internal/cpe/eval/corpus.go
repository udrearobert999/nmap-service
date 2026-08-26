package eval

type Sample struct {
	Product string
	Version string
	WantCPE string
	Source  string
	Note    string
}

func Corpus() []Sample {
	return []Sample{
		{
			Product: "OpenSSH",
			Version: "4.7p1 Debian 8ubuntu1",
			WantCPE: "cpe:2.3:a:openbsd:openssh:4.7p1:*:*:*:*:*:*:*",
			Source:  "metasploitable2",
			Note:    "distro-patched banner",
		},
		{
			Product: "Apache httpd",
			Version: "2.2.8 ((Ubuntu) DAV/2)",
			WantCPE: "cpe:2.3:a:apache:http_server:2.2.8:*:*:*:*:*:*:*",
			Source:  "metasploitable2",
			Note:    "trailing module text",
		},
		{
			Product: "PostgreSQL DB",
			Version: "8.3.0 - 8.3.7",
			WantCPE: "cpe:2.3:a:postgresql:postgresql:8.3.0:*:*:*:*:*:*:*",
			Source:  "metasploitable2",
			Note:    "version range banner",
		},
		{
			Product: "ProFTPD",
			Version: "1.3.1",
			WantCPE: "cpe:2.3:a:proftpd:proftpd:1.3.1:*:*:*:*:*:*:*",
			Source:  "metasploitable2",
		},
		{
			Product: "ISC BIND",
			Version: "9.4.2",
			WantCPE: "cpe:2.3:a:isc:bind:9.4.2:*:*:*:*:*:*:*",
			Source:  "metasploitable2",
		},
		{
			Product: "Postfix smtpd",
			Version: "",
			WantCPE: "cpe:2.3:a:postfix:postfix:*:*:*:*:*:*:*:*",
			Source:  "metasploitable2",
			Note:    "no version disclosed",
		},
		{
			Product: "Samba smbd",
			Version: "3.X - 4.X",
			WantCPE: "cpe:2.3:a:samba:samba:3:*:*:*:*:*:*:*",
			Source:  "metasploitable2",
			Note:    "wildcard major-version banner",
		},
		{
			Product: "MySQL",
			Version: "5.0.51a-3ubuntu5",
			WantCPE: "cpe:2.3:a:oracle:mysql:5.0.51a:*:*:*:*:*:*:*",
			Source:  "metasploitable2",
			Note:    "NVD vendor is oracle, not mysql",
		},
		{
			Product: "Apache Tomcat/Coyote JSP engine",
			Version: "1.1",
			WantCPE: "cpe:2.3:a:apache:tomcat:1.1:*:*:*:*:*:*:*",
			Source:  "metasploitable2",
			Note:    "compound product string",
		},
		{
			Product: "UnrealIRCd",
			Version: "",
			WantCPE: "cpe:2.3:a:unrealircd:unrealircd:*:*:*:*:*:*:*:*",
			Source:  "metasploitable2",
			Note:    "out of dictionary",
		},
		{
			Product: "Apache httpd",
			Version: "2.4.7",
			WantCPE: "cpe:2.3:a:apache:http_server:2.4.7:*:*:*:*:*:*:*",
			Source:  "dvwa",
		},
		{
			Product: "MariaDB",
			Version: "10.3.25",
			WantCPE: "cpe:2.3:a:mariadb:mariadb:10.3.25:*:*:*:*:*:*:*",
			Source:  "dvwa",
		},
		{
			Product: "OpenSSH",
			Version: "8.2p1 Ubuntu 4ubuntu0.5",
			WantCPE: "cpe:2.3:a:openbsd:openssh:8.2p1:*:*:*:*:*:*:*",
			Source:  "common",
			Note:    "distro-patched banner",
		},
		{
			Product: "nginx",
			Version: "1.18.0",
			WantCPE: "cpe:2.3:a:nginx:nginx:1.18.0:*:*:*:*:*:*:*",
			Source:  "common",
		},
		{
			Product: "Microsoft IIS httpd",
			Version: "10.0",
			WantCPE: "cpe:2.3:a:microsoft:internet_information_services:10.0:*:*:*:*:*:*:*",
			Source:  "common",
		},
		{
			Product: "Exim smtpd",
			Version: "4.94",
			WantCPE: "cpe:2.3:a:exim:exim:4.94:*:*:*:*:*:*:*",
			Source:  "common",
		},
		{
			Product: "Dropbear sshd",
			Version: "2019.78",
			WantCPE: "cpe:2.3:a:dropbear_ssh_project:dropbear_ssh:2019.78:*:*:*:*:*:*:*",
			Source:  "common",
		},
		{
			Product: "Redis",
			Version: "6.0.16",
			WantCPE: "cpe:2.3:a:redis:redis:6.0.16:*:*:*:*:*:*:*",
			Source:  "common",
		},
		{
			Product: "MongoDB",
			Version: "4.4.6",
			WantCPE: "cpe:2.3:a:mongodb:mongodb:4.4.6:*:*:*:*:*:*:*",
			Source:  "common",
		},
		{
			Product: "Elasticsearch",
			Version: "7.10.2",
			WantCPE: "cpe:2.3:a:elastic:elasticsearch:7.10.2:*:*:*:*:*:*:*",
			Source:  "common",
		},
		{
			Product: "lighttpd",
			Version: "1.4.55",
			WantCPE: "cpe:2.3:a:lighttpd:lighttpd:1.4.55:*:*:*:*:*:*:*",
			Source:  "common",
		},
		{
			Product: "Jetty",
			Version: "9.4.z-SNAPSHOT",
			WantCPE: "cpe:2.3:a:eclipse:jetty:9.4:*:*:*:*:*:*:*",
			Source:  "common",
			Note:    "snapshot suffix",
		},
		{
			Product: "vsftpd",
			Version: "2.3.4",
			WantCPE: "cpe:2.3:a:vsftpd:vsftpd:2.3.4:*:*:*:*:*:*:*",
			Source:  "metasploitable2",
		},
		{
			Product: "OpenLDAP",
			Version: "2.4.49",
			WantCPE: "cpe:2.3:a:openldap:openldap:2.4.49:*:*:*:*:*:*:*",
			Source:  "held-out",
			Note:    "not in dictionary; vendor equals product",
		},
		{
			Product: "HAProxy",
			Version: "2.2.9",
			WantCPE: "cpe:2.3:a:haproxy:haproxy:2.2.9:*:*:*:*:*:*:*",
			Source:  "held-out",
			Note:    "not in dictionary; vendor equals product",
		},
		{
			Product: "Memcached",
			Version: "1.6.9",
			WantCPE: "cpe:2.3:a:memcached:memcached:1.6.9:*:*:*:*:*:*:*",
			Source:  "held-out",
			Note:    "not in dictionary; vendor equals product",
		},
		{
			Product: "Squid http proxy",
			Version: "4.13",
			WantCPE: "cpe:2.3:a:squid-cache:squid:4.13:*:*:*:*:*:*:*",
			Source:  "held-out",
			Note:    "not in dictionary; vendor differs from product",
		},
		{
			Product: "Dovecot imapd",
			Version: "2.3.13",
			WantCPE: "cpe:2.3:a:dovecot:dovecot:2.3.13:*:*:*:*:*:*:*",
			Source:  "held-out",
			Note:    "not in dictionary; compound product string",
		},
		{
			Product: "ISC DHCPD",
			Version: "4.4.1",
			WantCPE: "cpe:2.3:a:isc:dhcp:4.4.1:*:*:*:*:*:*:*",
			Source:  "held-out",
			Note:    "not in dictionary; vendor differs from product",
		},
		{
			Product: "RabbitMQ",
			Version: "3.8.9",
			WantCPE: "cpe:2.3:a:pivotal_software:rabbitmq:3.8.9:*:*:*:*:*:*:*",
			Source:  "held-out",
			Note:    "not in dictionary; vendor differs from product",
		},
		{
			Product: "Werkzeug httpd",
			Version: "1.0.1",
			WantCPE: "cpe:2.3:a:palletsprojects:werkzeug:1.0.1:*:*:*:*:*:*:*",
			Source:  "held-out",
			Note:    "not in dictionary; vendor differs from product",
		},
		{
			Product: "",
			Version: "",
			WantCPE: "",
			Source:  "sandbox",
			Note:    "tcpwrapped: no product fingerprint",
		},
		{
			Product: "Metasploitable root shell",
			Version: "",
			WantCPE: "",
			Source:  "metasploitable2",
			Note:    "not a catalogued product",
		},
		{
			Product: "Netkit rshd",
			Version: "",
			WantCPE: "",
			Source:  "metasploitable2",
			Note:    "not a catalogued product",
		},
	}
}
