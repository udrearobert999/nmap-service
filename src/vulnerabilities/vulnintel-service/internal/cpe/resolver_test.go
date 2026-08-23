package cpe

import "testing"

func TestResolve(t *testing.T) {
	resolver := NewResolver()

	tests := []struct {
		name        string
		product     string
		version     string
		wantURI     string
		wantMinConf float64
	}{
		{
			name:        "known product with clean version",
			product:     "OpenSSH",
			version:     "6.6.1p1 Ubuntu 2ubuntu2.13",
			wantURI:     "cpe:2.3:a:openbsd:openssh:6.6.1p1:*:*:*:*:*:*:*",
			wantMinConf: 0.9,
		},
		{
			name:        "known product apache",
			product:     "Apache httpd",
			version:     "2.4.7",
			wantURI:     "cpe:2.3:a:apache:http_server:2.4.7:*:*:*:*:*:*:*",
			wantMinConf: 0.9,
		},
		{
			name:        "postgres with trailing text version",
			product:     "PostgreSQL DB",
			version:     "9.6.0 or later",
			wantURI:     "cpe:2.3:a:postgresql:postgresql:9.6.0:*:*:*:*:*:*:*",
			wantMinConf: 0.9,
		},
		{
			name:        "unknown product falls back with low confidence",
			product:     "SomeCustomDaemon",
			version:     "1.2.3",
			wantURI:     "cpe:2.3:a:somecustomdaemon:somecustomdaemon:1.2.3:*:*:*:*:*:*:*",
			wantMinConf: 0.4,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			got := resolver.Resolve(tt.product, tt.version)
			if got.URI != tt.wantURI {
				t.Errorf("URI = %q, want %q", got.URI, tt.wantURI)
			}
			if got.Confidence < tt.wantMinConf {
				t.Errorf("Confidence = %v, want >= %v", got.Confidence, tt.wantMinConf)
			}
		})
	}
}

func TestResolveEmptyProduct(t *testing.T) {
	resolver := NewResolver()
	got := resolver.Resolve("", "1.0")
	if got.URI != "" || got.Confidence != 0 {
		t.Errorf("empty product should yield empty CPE, got %+v", got)
	}
}
