-- Dev-only seed data: historical scans + risk assessments, so the portal has
-- enough history to demo the scan-diff and risk-trend views.
-- Apply against the default tenant (org_e2e / dev|e2e-user, for header-based
-- API testing) with:
--   docker exec -i vantage_db psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -f - < scripts/seed-dev-data.sql
-- Or against a specific team/user (e.g. a real signed-in Clerk org) with:
--   docker exec -i vantage_db psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" \
--     -v team_id="'<team-uuid>'" -v user_id="'<user-uuid>'" -f - < scripts/seed-dev-data.sql
-- Safe to re-run: it only deletes/recreates rows for the three synthetic
-- targets below (10.20.30.40-42) under the chosen team, never touches anything else.

BEGIN;

\if :{?team_id}
\else
\set team_id 'e4bf15a4-562d-461d-b18f-25869c56a6c6'
\endif
\if :{?user_id}
\else
\set user_id 'e00d4575-88c7-49b9-9d22-41a0a2b518f2'
\endif

DELETE FROM "Scans"
WHERE "TeamId" = :'team_id'
  AND "Target" IN ('10.20.30.40', '10.20.30.41', '10.20.30.42');

-- ============================================================
-- Target A: 10.20.30.40 — declining risk (remediation over time)
-- ============================================================

WITH scan AS (
  INSERT INTO "Scans" ("Id", "RequestId", "Target", "Status", "CreatedAt", "CompletedAt", "TeamId", "CreatedByUserId")
  VALUES (gen_random_uuid(), gen_random_uuid(), '10.20.30.40', 'Completed',
          now() - interval '75 days', now() - interval '75 days' + interval '3 minutes', :'team_id', :'user_id')
  RETURNING "Id", "CompletedAt"
), results AS (
  INSERT INTO "ScanResults" ("Id", "ScanId", "Port", "Protocol", "Service", "State", "Product", "Version")
  SELECT gen_random_uuid(), scan."Id", p, 'tcp', s, 'open', prod, ver
  FROM scan, (VALUES
    (22, 'ssh', 'OpenSSH', '4.7p1 Debian 8ubuntu1'),
    (21, 'ftp', 'vsftpd', '2.3.4'),
    (80, 'http', 'Apache httpd', '2.2.8 ((Ubuntu) DAV/2)')
  ) AS r(p, s, prod, ver)
  RETURNING "ScanId"
), assessment AS (
  INSERT INTO "ScanRiskAssessments" ("Id", "RequestId", "TeamId", "ScanId", "Status", "RequestedAt", "CompletedAt", "OverallRiskScore", "CreatedByUserId")
  SELECT gen_random_uuid(), gen_random_uuid(), :'team_id', scan."Id", 'Completed',
         scan."CompletedAt" + interval '5 seconds', scan."CompletedAt" + interval '30 seconds', 9.8, :'user_id'
  FROM scan
  RETURNING "Id"
)
INSERT INTO "ScanRiskAssessmentFindings" ("Id", "ScanRiskAssessmentId", "Port", "Service", "Product", "Version", "Cpe", "MatchedCves", "CvssScore", "KevFlag", "MatchConfidence")
SELECT gen_random_uuid(), assessment."Id", f.port, f.service, f.product, f.version, f.cpe, f.cves, f.cvss, f.kev, 0.9
FROM assessment, (VALUES
  (22, 'ssh', 'OpenSSH', '4.7p1 Debian 8ubuntu1', 'cpe:2.3:a:openbsd:openssh:4.7p1:*:*:*:*:*:*:*', 'CVE-2008-5161', 2.6, false),
  (21, 'ftp', 'vsftpd', '2.3.4', 'cpe:2.3:a:vsftpd:vsftpd:2.3.4:*:*:*:*:*:*:*', 'CVE-2011-2523', 9.8, true),
  (80, 'http', 'Apache httpd', '2.2.8 ((Ubuntu) DAV/2)', 'cpe:2.3:a:apache:http_server:2.2.8:*:*:*:*:*:*:*', 'CVE-2009-1891', 5.0, false)
) AS f(port, service, product, version, cpe, cves, cvss, kev);

WITH scan AS (
  INSERT INTO "Scans" ("Id", "RequestId", "Target", "Status", "CreatedAt", "CompletedAt", "TeamId", "CreatedByUserId")
  VALUES (gen_random_uuid(), gen_random_uuid(), '10.20.30.40', 'Completed',
          now() - interval '58 days', now() - interval '58 days' + interval '3 minutes', :'team_id', :'user_id')
  RETURNING "Id", "CompletedAt"
), results AS (
  INSERT INTO "ScanResults" ("Id", "ScanId", "Port", "Protocol", "Service", "State", "Product", "Version")
  SELECT gen_random_uuid(), scan."Id", p, 'tcp', s, 'open', prod, ver
  FROM scan, (VALUES
    (22, 'ssh', 'OpenSSH', '4.7p1 Debian 8ubuntu1'),
    (80, 'http', 'Apache httpd', '2.2.8 ((Ubuntu) DAV/2)')
  ) AS r(p, s, prod, ver)
  RETURNING "ScanId"
), assessment AS (
  INSERT INTO "ScanRiskAssessments" ("Id", "RequestId", "TeamId", "ScanId", "Status", "RequestedAt", "CompletedAt", "OverallRiskScore", "CreatedByUserId")
  SELECT gen_random_uuid(), gen_random_uuid(), :'team_id', scan."Id", 'Completed',
         scan."CompletedAt" + interval '5 seconds', scan."CompletedAt" + interval '30 seconds', 5.0, :'user_id'
  FROM scan
  RETURNING "Id"
)
INSERT INTO "ScanRiskAssessmentFindings" ("Id", "ScanRiskAssessmentId", "Port", "Service", "Product", "Version", "Cpe", "MatchedCves", "CvssScore", "KevFlag", "MatchConfidence")
SELECT gen_random_uuid(), assessment."Id", f.port, f.service, f.product, f.version, f.cpe, f.cves, f.cvss, f.kev, 0.9
FROM assessment, (VALUES
  (22, 'ssh', 'OpenSSH', '4.7p1 Debian 8ubuntu1', 'cpe:2.3:a:openbsd:openssh:4.7p1:*:*:*:*:*:*:*', 'CVE-2008-5161', 2.6, false),
  (80, 'http', 'Apache httpd', '2.2.8 ((Ubuntu) DAV/2)', 'cpe:2.3:a:apache:http_server:2.2.8:*:*:*:*:*:*:*', 'CVE-2009-1891', 5.0, false)
) AS f(port, service, product, version, cpe, cves, cvss, kev);

WITH scan AS (
  INSERT INTO "Scans" ("Id", "RequestId", "Target", "Status", "CreatedAt", "CompletedAt", "TeamId", "CreatedByUserId")
  VALUES (gen_random_uuid(), gen_random_uuid(), '10.20.30.40', 'Completed',
          now() - interval '42 days', now() - interval '42 days' + interval '3 minutes', :'team_id', :'user_id')
  RETURNING "Id", "CompletedAt"
), results AS (
  INSERT INTO "ScanResults" ("Id", "ScanId", "Port", "Protocol", "Service", "State", "Product", "Version")
  SELECT gen_random_uuid(), scan."Id", p, 'tcp', s, 'open', prod, ver
  FROM scan, (VALUES
    (22, 'ssh', 'OpenSSH', '4.7p1 Debian 8ubuntu1'),
    (80, 'http', 'Apache httpd', '2.4.41')
  ) AS r(p, s, prod, ver)
  RETURNING "ScanId"
), assessment AS (
  INSERT INTO "ScanRiskAssessments" ("Id", "RequestId", "TeamId", "ScanId", "Status", "RequestedAt", "CompletedAt", "OverallRiskScore", "CreatedByUserId")
  SELECT gen_random_uuid(), gen_random_uuid(), :'team_id', scan."Id", 'Completed',
         scan."CompletedAt" + interval '5 seconds', scan."CompletedAt" + interval '30 seconds', 2.6, :'user_id'
  FROM scan
  RETURNING "Id"
)
INSERT INTO "ScanRiskAssessmentFindings" ("Id", "ScanRiskAssessmentId", "Port", "Service", "Product", "Version", "Cpe", "MatchedCves", "CvssScore", "KevFlag", "MatchConfidence")
SELECT gen_random_uuid(), assessment."Id", f.port, f.service, f.product, f.version, f.cpe, f.cves, f.cvss, f.kev, 0.9
FROM assessment, (VALUES
  (22, 'ssh', 'OpenSSH', '4.7p1 Debian 8ubuntu1', 'cpe:2.3:a:openbsd:openssh:4.7p1:*:*:*:*:*:*:*', 'CVE-2008-5161', 2.6, false),
  (80, 'http', 'Apache httpd', '2.4.41', 'cpe:2.3:a:apache:http_server:2.4.41:*:*:*:*:*:*:*', '', 0.0, false)
) AS f(port, service, product, version, cpe, cves, cvss, kev);

WITH scan AS (
  INSERT INTO "Scans" ("Id", "RequestId", "Target", "Status", "CreatedAt", "CompletedAt", "TeamId", "CreatedByUserId")
  VALUES (gen_random_uuid(), gen_random_uuid(), '10.20.30.40', 'Completed',
          now() - interval '26 days', now() - interval '26 days' + interval '3 minutes', :'team_id', :'user_id')
  RETURNING "Id", "CompletedAt"
), results AS (
  INSERT INTO "ScanResults" ("Id", "ScanId", "Port", "Protocol", "Service", "State", "Product", "Version")
  SELECT gen_random_uuid(), scan."Id", p, 'tcp', s, 'open', prod, ver
  FROM scan, (VALUES
    (22, 'ssh', 'OpenSSH', '8.2p1 Ubuntu 4ubuntu0.5'),
    (80, 'http', 'Apache httpd', '2.4.41')
  ) AS r(p, s, prod, ver)
  RETURNING "ScanId"
), assessment AS (
  INSERT INTO "ScanRiskAssessments" ("Id", "RequestId", "TeamId", "ScanId", "Status", "RequestedAt", "CompletedAt", "OverallRiskScore", "CreatedByUserId")
  SELECT gen_random_uuid(), gen_random_uuid(), :'team_id', scan."Id", 'Completed',
         scan."CompletedAt" + interval '5 seconds', scan."CompletedAt" + interval '30 seconds', 0.0, :'user_id'
  FROM scan
  RETURNING "Id"
)
INSERT INTO "ScanRiskAssessmentFindings" ("Id", "ScanRiskAssessmentId", "Port", "Service", "Product", "Version", "Cpe", "MatchedCves", "CvssScore", "KevFlag", "MatchConfidence")
SELECT gen_random_uuid(), assessment."Id", f.port, f.service, f.product, f.version, f.cpe, f.cves, f.cvss, f.kev, 0.9
FROM assessment, (VALUES
  (22, 'ssh', 'OpenSSH', '8.2p1 Ubuntu 4ubuntu0.5', 'cpe:2.3:a:openbsd:openssh:8.2p1:*:*:*:*:*:*:*', '', 0.0, false),
  (80, 'http', 'Apache httpd', '2.4.41', 'cpe:2.3:a:apache:http_server:2.4.41:*:*:*:*:*:*:*', '', 0.0, false)
) AS f(port, service, product, version, cpe, cves, cvss, kev);

WITH scan AS (
  INSERT INTO "Scans" ("Id", "RequestId", "Target", "Status", "CreatedAt", "CompletedAt", "TeamId", "CreatedByUserId")
  VALUES (gen_random_uuid(), gen_random_uuid(), '10.20.30.40', 'Completed',
          now() - interval '12 days', now() - interval '12 days' + interval '3 minutes', :'team_id', :'user_id')
  RETURNING "Id", "CompletedAt"
), results AS (
  INSERT INTO "ScanResults" ("Id", "ScanId", "Port", "Protocol", "Service", "State", "Product", "Version")
  SELECT gen_random_uuid(), scan."Id", p, 'tcp', s, 'open', prod, ver
  FROM scan, (VALUES
    (22, 'ssh', 'OpenSSH', '8.2p1 Ubuntu 4ubuntu0.5'),
    (80, 'http', 'Apache httpd', '2.4.41')
  ) AS r(p, s, prod, ver)
  RETURNING "ScanId"
), assessment AS (
  INSERT INTO "ScanRiskAssessments" ("Id", "RequestId", "TeamId", "ScanId", "Status", "RequestedAt", "CompletedAt", "OverallRiskScore", "CreatedByUserId")
  SELECT gen_random_uuid(), gen_random_uuid(), :'team_id', scan."Id", 'Completed',
         scan."CompletedAt" + interval '5 seconds', scan."CompletedAt" + interval '30 seconds', 0.0, :'user_id'
  FROM scan
  RETURNING "Id"
)
INSERT INTO "ScanRiskAssessmentFindings" ("Id", "ScanRiskAssessmentId", "Port", "Service", "Product", "Version", "Cpe", "MatchedCves", "CvssScore", "KevFlag", "MatchConfidence")
SELECT gen_random_uuid(), assessment."Id", f.port, f.service, f.product, f.version, f.cpe, f.cves, f.cvss, f.kev, 0.9
FROM assessment, (VALUES
  (22, 'ssh', 'OpenSSH', '8.2p1 Ubuntu 4ubuntu0.5', 'cpe:2.3:a:openbsd:openssh:8.2p1:*:*:*:*:*:*:*', '', 0.0, false),
  (80, 'http', 'Apache httpd', '2.4.41', 'cpe:2.3:a:apache:http_server:2.4.41:*:*:*:*:*:*:*', '', 0.0, false)
) AS f(port, service, product, version, cpe, cves, cvss, kev);

WITH scan AS (
  INSERT INTO "Scans" ("Id", "RequestId", "Target", "Status", "CreatedAt", "CompletedAt", "TeamId", "CreatedByUserId")
  VALUES (gen_random_uuid(), gen_random_uuid(), '10.20.30.40', 'Completed',
          now() - interval '1 day', now() - interval '1 day' + interval '3 minutes', :'team_id', :'user_id')
  RETURNING "Id", "CompletedAt"
), results AS (
  INSERT INTO "ScanResults" ("Id", "ScanId", "Port", "Protocol", "Service", "State", "Product", "Version")
  SELECT gen_random_uuid(), scan."Id", p, 'tcp', s, 'open', prod, ver
  FROM scan, (VALUES
    (22, 'ssh', 'OpenSSH', '8.2p1 Ubuntu 4ubuntu0.5'),
    (80, 'http', 'Apache httpd', '2.4.41')
  ) AS r(p, s, prod, ver)
  RETURNING "ScanId"
), assessment AS (
  INSERT INTO "ScanRiskAssessments" ("Id", "RequestId", "TeamId", "ScanId", "Status", "RequestedAt", "CompletedAt", "OverallRiskScore", "CreatedByUserId")
  SELECT gen_random_uuid(), gen_random_uuid(), :'team_id', scan."Id", 'Completed',
         scan."CompletedAt" + interval '5 seconds', scan."CompletedAt" + interval '30 seconds', 0.0, :'user_id'
  FROM scan
  RETURNING "Id"
)
INSERT INTO "ScanRiskAssessmentFindings" ("Id", "ScanRiskAssessmentId", "Port", "Service", "Product", "Version", "Cpe", "MatchedCves", "CvssScore", "KevFlag", "MatchConfidence")
SELECT gen_random_uuid(), assessment."Id", f.port, f.service, f.product, f.version, f.cpe, f.cves, f.cvss, f.kev, 0.9
FROM assessment, (VALUES
  (22, 'ssh', 'OpenSSH', '8.2p1 Ubuntu 4ubuntu0.5', 'cpe:2.3:a:openbsd:openssh:8.2p1:*:*:*:*:*:*:*', '', 0.0, false),
  (80, 'http', 'Apache httpd', '2.4.41', 'cpe:2.3:a:apache:http_server:2.4.41:*:*:*:*:*:*:*', '', 0.0, false)
) AS f(port, service, product, version, cpe, cves, cvss, kev);

-- ============================================================
-- Target B: 10.20.30.41 — rising risk (Redis exposed without auth)
-- ============================================================

WITH scan AS (
  INSERT INTO "Scans" ("Id", "RequestId", "Target", "Status", "CreatedAt", "CompletedAt", "TeamId", "CreatedByUserId")
  VALUES (gen_random_uuid(), gen_random_uuid(), '10.20.30.41', 'Completed',
          now() - interval '50 days', now() - interval '50 days' + interval '3 minutes', :'team_id', :'user_id')
  RETURNING "Id", "CompletedAt"
), results AS (
  INSERT INTO "ScanResults" ("Id", "ScanId", "Port", "Protocol", "Service", "State", "Product", "Version")
  SELECT gen_random_uuid(), scan."Id", p, 'tcp', s, 'open', prod, ver
  FROM scan, (VALUES
    (22, 'ssh', 'OpenSSH', '8.4p1'),
    (3306, 'mysql', 'MySQL', '8.0.21')
  ) AS r(p, s, prod, ver)
  RETURNING "ScanId"
), assessment AS (
  INSERT INTO "ScanRiskAssessments" ("Id", "RequestId", "TeamId", "ScanId", "Status", "RequestedAt", "CompletedAt", "OverallRiskScore", "CreatedByUserId")
  SELECT gen_random_uuid(), gen_random_uuid(), :'team_id', scan."Id", 'Completed',
         scan."CompletedAt" + interval '5 seconds', scan."CompletedAt" + interval '30 seconds', 0.0, :'user_id'
  FROM scan
  RETURNING "Id"
)
INSERT INTO "ScanRiskAssessmentFindings" ("Id", "ScanRiskAssessmentId", "Port", "Service", "Product", "Version", "Cpe", "MatchedCves", "CvssScore", "KevFlag", "MatchConfidence")
SELECT gen_random_uuid(), assessment."Id", f.port, f.service, f.product, f.version, f.cpe, f.cves, f.cvss, f.kev, 0.9
FROM assessment, (VALUES
  (22, 'ssh', 'OpenSSH', '8.4p1', 'cpe:2.3:a:openbsd:openssh:8.4p1:*:*:*:*:*:*:*', '', 0.0, false),
  (3306, 'mysql', 'MySQL', '8.0.21', 'cpe:2.3:a:oracle:mysql:8.0.21:*:*:*:*:*:*:*', '', 0.0, false)
) AS f(port, service, product, version, cpe, cves, cvss, kev);

WITH scan AS (
  INSERT INTO "Scans" ("Id", "RequestId", "Target", "Status", "CreatedAt", "CompletedAt", "TeamId", "CreatedByUserId")
  VALUES (gen_random_uuid(), gen_random_uuid(), '10.20.30.41', 'Completed',
          now() - interval '37 days', now() - interval '37 days' + interval '3 minutes', :'team_id', :'user_id')
  RETURNING "Id", "CompletedAt"
), results AS (
  INSERT INTO "ScanResults" ("Id", "ScanId", "Port", "Protocol", "Service", "State", "Product", "Version")
  SELECT gen_random_uuid(), scan."Id", p, 'tcp', s, 'open', prod, ver
  FROM scan, (VALUES
    (22, 'ssh', 'OpenSSH', '8.4p1'),
    (3306, 'mysql', 'MySQL', '8.0.21')
  ) AS r(p, s, prod, ver)
  RETURNING "ScanId"
), assessment AS (
  INSERT INTO "ScanRiskAssessments" ("Id", "RequestId", "TeamId", "ScanId", "Status", "RequestedAt", "CompletedAt", "OverallRiskScore", "CreatedByUserId")
  SELECT gen_random_uuid(), gen_random_uuid(), :'team_id', scan."Id", 'Completed',
         scan."CompletedAt" + interval '5 seconds', scan."CompletedAt" + interval '30 seconds', 0.0, :'user_id'
  FROM scan
  RETURNING "Id"
)
INSERT INTO "ScanRiskAssessmentFindings" ("Id", "ScanRiskAssessmentId", "Port", "Service", "Product", "Version", "Cpe", "MatchedCves", "CvssScore", "KevFlag", "MatchConfidence")
SELECT gen_random_uuid(), assessment."Id", f.port, f.service, f.product, f.version, f.cpe, f.cves, f.cvss, f.kev, 0.9
FROM assessment, (VALUES
  (22, 'ssh', 'OpenSSH', '8.4p1', 'cpe:2.3:a:openbsd:openssh:8.4p1:*:*:*:*:*:*:*', '', 0.0, false),
  (3306, 'mysql', 'MySQL', '8.0.21', 'cpe:2.3:a:oracle:mysql:8.0.21:*:*:*:*:*:*:*', '', 0.0, false)
) AS f(port, service, product, version, cpe, cves, cvss, kev);

WITH scan AS (
  INSERT INTO "Scans" ("Id", "RequestId", "Target", "Status", "CreatedAt", "CompletedAt", "TeamId", "CreatedByUserId")
  VALUES (gen_random_uuid(), gen_random_uuid(), '10.20.30.41', 'Completed',
          now() - interval '24 days', now() - interval '24 days' + interval '3 minutes', :'team_id', :'user_id')
  RETURNING "Id", "CompletedAt"
), results AS (
  INSERT INTO "ScanResults" ("Id", "ScanId", "Port", "Protocol", "Service", "State", "Product", "Version")
  SELECT gen_random_uuid(), scan."Id", p, 'tcp', s, 'open', prod, ver
  FROM scan, (VALUES
    (22, 'ssh', 'OpenSSH', '8.4p1'),
    (3306, 'mysql', 'MySQL', '8.0.21')
  ) AS r(p, s, prod, ver)
  RETURNING "ScanId"
), assessment AS (
  INSERT INTO "ScanRiskAssessments" ("Id", "RequestId", "TeamId", "ScanId", "Status", "RequestedAt", "CompletedAt", "OverallRiskScore", "CreatedByUserId")
  SELECT gen_random_uuid(), gen_random_uuid(), :'team_id', scan."Id", 'Completed',
         scan."CompletedAt" + interval '5 seconds', scan."CompletedAt" + interval '30 seconds', 0.0, :'user_id'
  FROM scan
  RETURNING "Id"
)
INSERT INTO "ScanRiskAssessmentFindings" ("Id", "ScanRiskAssessmentId", "Port", "Service", "Product", "Version", "Cpe", "MatchedCves", "CvssScore", "KevFlag", "MatchConfidence")
SELECT gen_random_uuid(), assessment."Id", f.port, f.service, f.product, f.version, f.cpe, f.cves, f.cvss, f.kev, 0.9
FROM assessment, (VALUES
  (22, 'ssh', 'OpenSSH', '8.4p1', 'cpe:2.3:a:openbsd:openssh:8.4p1:*:*:*:*:*:*:*', '', 0.0, false),
  (3306, 'mysql', 'MySQL', '8.0.21', 'cpe:2.3:a:oracle:mysql:8.0.21:*:*:*:*:*:*:*', '', 0.0, false)
) AS f(port, service, product, version, cpe, cves, cvss, kev);

WITH scan AS (
  INSERT INTO "Scans" ("Id", "RequestId", "Target", "Status", "CreatedAt", "CompletedAt", "TeamId", "CreatedByUserId")
  VALUES (gen_random_uuid(), gen_random_uuid(), '10.20.30.41', 'Completed',
          now() - interval '11 days', now() - interval '11 days' + interval '3 minutes', :'team_id', :'user_id')
  RETURNING "Id", "CompletedAt"
), results AS (
  INSERT INTO "ScanResults" ("Id", "ScanId", "Port", "Protocol", "Service", "State", "Product", "Version")
  SELECT gen_random_uuid(), scan."Id", p, 'tcp', s, 'open', prod, ver
  FROM scan, (VALUES
    (22, 'ssh', 'OpenSSH', '8.4p1'),
    (3306, 'mysql', 'MySQL', '8.0.21'),
    (6379, 'redis', 'Redis', '5.0.7')
  ) AS r(p, s, prod, ver)
  RETURNING "ScanId"
), assessment AS (
  INSERT INTO "ScanRiskAssessments" ("Id", "RequestId", "TeamId", "ScanId", "Status", "RequestedAt", "CompletedAt", "OverallRiskScore", "CreatedByUserId")
  SELECT gen_random_uuid(), gen_random_uuid(), :'team_id', scan."Id", 'Completed',
         scan."CompletedAt" + interval '5 seconds', scan."CompletedAt" + interval '30 seconds', 10.0, :'user_id'
  FROM scan
  RETURNING "Id"
)
INSERT INTO "ScanRiskAssessmentFindings" ("Id", "ScanRiskAssessmentId", "Port", "Service", "Product", "Version", "Cpe", "MatchedCves", "CvssScore", "KevFlag", "MatchConfidence")
SELECT gen_random_uuid(), assessment."Id", f.port, f.service, f.product, f.version, f.cpe, f.cves, f.cvss, f.kev, 0.9
FROM assessment, (VALUES
  (22, 'ssh', 'OpenSSH', '8.4p1', 'cpe:2.3:a:openbsd:openssh:8.4p1:*:*:*:*:*:*:*', '', 0.0, false),
  (3306, 'mysql', 'MySQL', '8.0.21', 'cpe:2.3:a:oracle:mysql:8.0.21:*:*:*:*:*:*:*', '', 0.0, false),
  (6379, 'redis', 'Redis', '5.0.7', 'cpe:2.3:a:redis:redis:5.0.7:*:*:*:*:*:*:*', 'CVE-2022-0543', 10.0, true)
) AS f(port, service, product, version, cpe, cves, cvss, kev);

WITH scan AS (
  INSERT INTO "Scans" ("Id", "RequestId", "Target", "Status", "CreatedAt", "CompletedAt", "TeamId", "CreatedByUserId")
  VALUES (gen_random_uuid(), gen_random_uuid(), '10.20.30.41', 'Completed',
          now() - interval '3 days', now() - interval '3 days' + interval '3 minutes', :'team_id', :'user_id')
  RETURNING "Id", "CompletedAt"
), results AS (
  INSERT INTO "ScanResults" ("Id", "ScanId", "Port", "Protocol", "Service", "State", "Product", "Version")
  SELECT gen_random_uuid(), scan."Id", p, 'tcp', s, 'open', prod, ver
  FROM scan, (VALUES
    (22, 'ssh', 'OpenSSH', '8.4p1'),
    (3306, 'mysql', 'MySQL', '8.0.21'),
    (6379, 'redis', 'Redis', '5.0.7')
  ) AS r(p, s, prod, ver)
  RETURNING "ScanId"
), assessment AS (
  INSERT INTO "ScanRiskAssessments" ("Id", "RequestId", "TeamId", "ScanId", "Status", "RequestedAt", "CompletedAt", "OverallRiskScore", "CreatedByUserId")
  SELECT gen_random_uuid(), gen_random_uuid(), :'team_id', scan."Id", 'Completed',
         scan."CompletedAt" + interval '5 seconds', scan."CompletedAt" + interval '30 seconds', 10.0, :'user_id'
  FROM scan
  RETURNING "Id"
)
INSERT INTO "ScanRiskAssessmentFindings" ("Id", "ScanRiskAssessmentId", "Port", "Service", "Product", "Version", "Cpe", "MatchedCves", "CvssScore", "KevFlag", "MatchConfidence")
SELECT gen_random_uuid(), assessment."Id", f.port, f.service, f.product, f.version, f.cpe, f.cves, f.cvss, f.kev, 0.9
FROM assessment, (VALUES
  (22, 'ssh', 'OpenSSH', '8.4p1', 'cpe:2.3:a:openbsd:openssh:8.4p1:*:*:*:*:*:*:*', '', 0.0, false),
  (3306, 'mysql', 'MySQL', '8.0.21', 'cpe:2.3:a:oracle:mysql:8.0.21:*:*:*:*:*:*:*', '', 0.0, false),
  (6379, 'redis', 'Redis', '5.0.7', 'cpe:2.3:a:redis:redis:5.0.7:*:*:*:*:*:*:*', 'CVE-2022-0543', 10.0, true)
) AS f(port, service, product, version, cpe, cves, cvss, kev);

-- ============================================================
-- Target C: 10.20.30.42 — stable, healthy baseline
-- ============================================================

WITH scan AS (
  INSERT INTO "Scans" ("Id", "RequestId", "Target", "Status", "CreatedAt", "CompletedAt", "TeamId", "CreatedByUserId")
  VALUES (gen_random_uuid(), gen_random_uuid(), '10.20.30.42', 'Completed',
          now() - interval '40 days', now() - interval '40 days' + interval '3 minutes', :'team_id', :'user_id')
  RETURNING "Id", "CompletedAt"
), results AS (
  INSERT INTO "ScanResults" ("Id", "ScanId", "Port", "Protocol", "Service", "State", "Product", "Version")
  SELECT gen_random_uuid(), scan."Id", p, 'tcp', s, 'open', prod, ver
  FROM scan, (VALUES
    (22, 'ssh', 'OpenSSH', '9.0p1'),
    (443, 'https', 'nginx', '1.24.0')
  ) AS r(p, s, prod, ver)
  RETURNING "ScanId"
), assessment AS (
  INSERT INTO "ScanRiskAssessments" ("Id", "RequestId", "TeamId", "ScanId", "Status", "RequestedAt", "CompletedAt", "OverallRiskScore", "CreatedByUserId")
  SELECT gen_random_uuid(), gen_random_uuid(), :'team_id', scan."Id", 'Completed',
         scan."CompletedAt" + interval '5 seconds', scan."CompletedAt" + interval '30 seconds', 0.0, :'user_id'
  FROM scan
  RETURNING "Id"
)
INSERT INTO "ScanRiskAssessmentFindings" ("Id", "ScanRiskAssessmentId", "Port", "Service", "Product", "Version", "Cpe", "MatchedCves", "CvssScore", "KevFlag", "MatchConfidence")
SELECT gen_random_uuid(), assessment."Id", f.port, f.service, f.product, f.version, f.cpe, f.cves, f.cvss, f.kev, 0.9
FROM assessment, (VALUES
  (22, 'ssh', 'OpenSSH', '9.0p1', 'cpe:2.3:a:openbsd:openssh:9.0p1:*:*:*:*:*:*:*', '', 0.0, false),
  (443, 'https', 'nginx', '1.24.0', 'cpe:2.3:a:nginx:nginx:1.24.0:*:*:*:*:*:*:*', '', 0.0, false)
) AS f(port, service, product, version, cpe, cves, cvss, kev);

WITH scan AS (
  INSERT INTO "Scans" ("Id", "RequestId", "Target", "Status", "CreatedAt", "CompletedAt", "TeamId", "CreatedByUserId")
  VALUES (gen_random_uuid(), gen_random_uuid(), '10.20.30.42', 'Completed',
          now() - interval '27 days', now() - interval '27 days' + interval '3 minutes', :'team_id', :'user_id')
  RETURNING "Id", "CompletedAt"
), results AS (
  INSERT INTO "ScanResults" ("Id", "ScanId", "Port", "Protocol", "Service", "State", "Product", "Version")
  SELECT gen_random_uuid(), scan."Id", p, 'tcp', s, 'open', prod, ver
  FROM scan, (VALUES
    (22, 'ssh', 'OpenSSH', '9.0p1'),
    (443, 'https', 'nginx', '1.24.0')
  ) AS r(p, s, prod, ver)
  RETURNING "ScanId"
), assessment AS (
  INSERT INTO "ScanRiskAssessments" ("Id", "RequestId", "TeamId", "ScanId", "Status", "RequestedAt", "CompletedAt", "OverallRiskScore", "CreatedByUserId")
  SELECT gen_random_uuid(), gen_random_uuid(), :'team_id', scan."Id", 'Completed',
         scan."CompletedAt" + interval '5 seconds', scan."CompletedAt" + interval '30 seconds', 0.0, :'user_id'
  FROM scan
  RETURNING "Id"
)
INSERT INTO "ScanRiskAssessmentFindings" ("Id", "ScanRiskAssessmentId", "Port", "Service", "Product", "Version", "Cpe", "MatchedCves", "CvssScore", "KevFlag", "MatchConfidence")
SELECT gen_random_uuid(), assessment."Id", f.port, f.service, f.product, f.version, f.cpe, f.cves, f.cvss, f.kev, 0.9
FROM assessment, (VALUES
  (22, 'ssh', 'OpenSSH', '9.0p1', 'cpe:2.3:a:openbsd:openssh:9.0p1:*:*:*:*:*:*:*', '', 0.0, false),
  (443, 'https', 'nginx', '1.24.0', 'cpe:2.3:a:nginx:nginx:1.24.0:*:*:*:*:*:*:*', '', 0.0, false)
) AS f(port, service, product, version, cpe, cves, cvss, kev);

WITH scan AS (
  INSERT INTO "Scans" ("Id", "RequestId", "Target", "Status", "CreatedAt", "CompletedAt", "TeamId", "CreatedByUserId")
  VALUES (gen_random_uuid(), gen_random_uuid(), '10.20.30.42', 'Completed',
          now() - interval '14 days', now() - interval '14 days' + interval '3 minutes', :'team_id', :'user_id')
  RETURNING "Id", "CompletedAt"
), results AS (
  INSERT INTO "ScanResults" ("Id", "ScanId", "Port", "Protocol", "Service", "State", "Product", "Version")
  SELECT gen_random_uuid(), scan."Id", p, 'tcp', s, 'open', prod, ver
  FROM scan, (VALUES
    (22, 'ssh', 'OpenSSH', '9.0p1'),
    (443, 'https', 'nginx', '1.24.0')
  ) AS r(p, s, prod, ver)
  RETURNING "ScanId"
), assessment AS (
  INSERT INTO "ScanRiskAssessments" ("Id", "RequestId", "TeamId", "ScanId", "Status", "RequestedAt", "CompletedAt", "OverallRiskScore", "CreatedByUserId")
  SELECT gen_random_uuid(), gen_random_uuid(), :'team_id', scan."Id", 'Completed',
         scan."CompletedAt" + interval '5 seconds', scan."CompletedAt" + interval '30 seconds', 0.0, :'user_id'
  FROM scan
  RETURNING "Id"
)
INSERT INTO "ScanRiskAssessmentFindings" ("Id", "ScanRiskAssessmentId", "Port", "Service", "Product", "Version", "Cpe", "MatchedCves", "CvssScore", "KevFlag", "MatchConfidence")
SELECT gen_random_uuid(), assessment."Id", f.port, f.service, f.product, f.version, f.cpe, f.cves, f.cvss, f.kev, 0.9
FROM assessment, (VALUES
  (22, 'ssh', 'OpenSSH', '9.0p1', 'cpe:2.3:a:openbsd:openssh:9.0p1:*:*:*:*:*:*:*', '', 0.0, false),
  (443, 'https', 'nginx', '1.24.0', 'cpe:2.3:a:nginx:nginx:1.24.0:*:*:*:*:*:*:*', '', 0.0, false)
) AS f(port, service, product, version, cpe, cves, cvss, kev);

WITH scan AS (
  INSERT INTO "Scans" ("Id", "RequestId", "Target", "Status", "CreatedAt", "CompletedAt", "TeamId", "CreatedByUserId")
  VALUES (gen_random_uuid(), gen_random_uuid(), '10.20.30.42', 'Completed',
          now() - interval '1 day', now() - interval '1 day' + interval '3 minutes', :'team_id', :'user_id')
  RETURNING "Id", "CompletedAt"
), results AS (
  INSERT INTO "ScanResults" ("Id", "ScanId", "Port", "Protocol", "Service", "State", "Product", "Version")
  SELECT gen_random_uuid(), scan."Id", p, 'tcp', s, 'open', prod, ver
  FROM scan, (VALUES
    (22, 'ssh', 'OpenSSH', '9.0p1'),
    (443, 'https', 'nginx', '1.24.0')
  ) AS r(p, s, prod, ver)
  RETURNING "ScanId"
), assessment AS (
  INSERT INTO "ScanRiskAssessments" ("Id", "RequestId", "TeamId", "ScanId", "Status", "RequestedAt", "CompletedAt", "OverallRiskScore", "CreatedByUserId")
  SELECT gen_random_uuid(), gen_random_uuid(), :'team_id', scan."Id", 'Completed',
         scan."CompletedAt" + interval '5 seconds', scan."CompletedAt" + interval '30 seconds', 0.0, :'user_id'
  FROM scan
  RETURNING "Id"
)
INSERT INTO "ScanRiskAssessmentFindings" ("Id", "ScanRiskAssessmentId", "Port", "Service", "Product", "Version", "Cpe", "MatchedCves", "CvssScore", "KevFlag", "MatchConfidence")
SELECT gen_random_uuid(), assessment."Id", f.port, f.service, f.product, f.version, f.cpe, f.cves, f.cvss, f.kev, 0.9
FROM assessment, (VALUES
  (22, 'ssh', 'OpenSSH', '9.0p1', 'cpe:2.3:a:openbsd:openssh:9.0p1:*:*:*:*:*:*:*', '', 0.0, false),
  (443, 'https', 'nginx', '1.24.0', 'cpe:2.3:a:nginx:nginx:1.24.0:*:*:*:*:*:*:*', '', 0.0, false)
) AS f(port, service, product, version, cpe, cves, cvss, kev);

COMMIT;
