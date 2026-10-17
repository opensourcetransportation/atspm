import type { GitHubReleaseDto } from '@/api/config'
import {
  useGetVersionCurrentVersion,
  useGetVersionLatestVersionFromPreRelease,
} from '@/api/config'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import KeyboardDoubleArrowRightIcon from '@mui/icons-material/KeyboardDoubleArrowRight'
import OpenInNewIcon from '@mui/icons-material/OpenInNew'
import {
  Alert,
  AlertTitle,
  Box,
  Button,
  Divider,
  Grid,
  Paper,
  Stack,
  Typography,
} from '@mui/material'
import Image from 'next/image'
import Link from 'next/link'
import compareVersions from 'semver/functions/compare'
import parseVersion from 'semver/functions/parse'

type Partner = {
  path: string
  title: string
}

type PartnerSection = {
  title: string
  partners: Partner[]
}

const udotLogo: Partner = {
  path: '/images/udot.webp',
  title: 'UDOT logo',
}

const agencyPartners: Partner[] = [
  {
    path: '/images/partners/gdot.png',
    title: 'GDOT logo',
  },
  {
    path: '/images/partners/vdot.png',
    title: 'VDOT logo',
  },
  {
    path: '/images/partners/indot.png',
    title: 'INDOT logo',
  },
  {
    path: '/images/partners/fdot.png',
    title: 'FDOT logo',
  },
  {
    path: '/images/partners/minnesota-dot.png',
    title: 'Minnesota DOT logo',
  },
  {
    path: '/images/partners/north-carolina-dot.png',
    title: 'North Carolina DOT logo',
  },
  {
    path: '/images/partners/pennsylvania-dot.png',
    title: 'Pennsylvania DOT logo',
  },
  {
    path: '/images/partners/portland-oregon.svg',
    title: 'Portland Oregon logo',
  },
  {
    path: '/images/partners/vtrans.png',
    title: 'Vermont Trans logo',
  },
  {
    path: '/images/partners/maricopa-county.png',
    title: 'Maricopa County logo',
  },
  {
    path: '/images/partners/gilbert.png',
    title: 'Gilbert logo',
  },
  {
    path: '/images/partners/frisco.png',
    title: 'Frisco logo',
  },
]

const academicPartners: Partner[] = [
  {
    path: '/images/partners/purdue-university.png',
    title: 'Purdue University logo',
  },
  {
    path: '/images/partners/byu.svg',
    title: 'BYU logo',
  },
  {
    path: '/images/partners/university-of-utah.png',
    title: 'University of Utah logo',
  },
  {
    path: '/images/partners/university-of-texas-at-arlington.png',
    title: 'University of Texas at Arlington logo',
  },
  {
    path: '/images/partners/iowa-state-university.png',
    title: 'Iowa State University logo',
  },
  {
    path: '/images/partners/university-of-alabama.png',
    title: 'University of Alabama logo',
  },
  {
    path: '/images/partners/utah-state-university.png',
    title: 'Utah State University logo',
  },
]

const consultantPartners: Partner[] = [
  {
    path: '/images/partners/econolite.png',
    title: 'Econolite logo',
  },
  {
    path: '/images/partners/avenue-consultants.png',
    title: 'Avenue Consultants logo',
  },
  {
    path: '/images/partners/kittleson-and-associates.png',
    title: 'Kittleson and Associates logo',
  },
  {
    path: '/images/partners/atkins.png',
    title: 'Atkins logo',
  },
  {
    path: '/images/partners/kimley-horn.svg',
    title: 'Kimley Horn logo',
  },
  {
    path: '/images/partners/pinetop-engineering.jpg',
    title: 'Pinetop Engineering logo',
  },
  {
    path: '/images/partners/qfree.png',
    title: 'Q-Free logo',
  },
  {
    path: '/images/partners/miovision.png',
    title: 'Miovision logo',
  },
  {
    path: '/images/partners/lee-engineering.png',
    title: 'Lee Engineering logo',
  },
  {
    path: '/images/partners/traffop.png',
    title: 'Traffop logo',
  },
]

const partnerSections: PartnerSection[] = [
  {
    title: 'Agencies',
    partners: agencyPartners,
  },
  {
    title: 'Academics',
    partners: academicPartners,
  },
  {
    title: 'Consultants',
    partners: consultantPartners,
  },
]

const getReleaseLabel = (release?: GitHubReleaseDto) =>
  release?.tagName || release?.name || 'Unknown'

const parseReleaseVersion = (release?: GitHubReleaseDto) =>
  parseVersion(getReleaseLabel(release).trim().replace(/^v/i, ''))

const getReleaseSource = (release?: GitHubReleaseDto) => {
  try {
    const url = new URL(release?.sourceRepositoryUrl || release?.htmlUrl || '')
    const match = url.pathname.match(
      /^\/([\w.-]+)\/([\w.-]+)(?:\/releases(?:\/.*)?|\/)?$/
    )
    if (url.protocol !== 'https:' || url.hostname !== 'github.com' || !match) {
      return null
    }
    const name = `${match[1]}/${match[2]}`
    return { name, url: `https://github.com/${name}` }
  } catch {
    return null
  }
}

const VersionMetric = ({ label, value }: { label: string; value: string }) => (
  <Box>
    <Typography variant="caption" color="text.secondary">
      {label}
    </Typography>
    <Typography
      variant="body2"
      sx={{ fontFamily: 'monospace', fontWeight: 700 }}
    >
      {value}
    </Typography>
  </Box>
)

const VersionStatus = ({
  currentVersionLabel,
  latestVersionLabel,
  hasNewerVersion,
  isAheadOfStable,
  isComparisonUnavailable,
  isInstalledPrerelease,
  releaseSource,
  isVersionLoading,
  isVersionUnavailable,
  releaseUrl,
}: {
  currentVersionLabel: string
  latestVersionLabel: string
  hasNewerVersion: boolean
  isAheadOfStable: boolean
  isComparisonUnavailable: boolean
  isInstalledPrerelease: boolean
  releaseSource: ReturnType<typeof getReleaseSource>
  isVersionLoading: boolean
  isVersionUnavailable: boolean
  releaseUrl?: string | null
}) => {
  const versionSeverity = isVersionLoading
    ? 'info'
    : isVersionUnavailable
      ? 'error'
      : hasNewerVersion
        ? 'warning'
        : isComparisonUnavailable || isAheadOfStable
          ? 'info'
          : 'success'
  const versionTitle = isVersionLoading
    ? 'Checking version status'
    : isVersionUnavailable
      ? 'Version status unavailable'
      : hasNewerVersion
        ? 'Update available'
        : isComparisonUnavailable
          ? 'Version comparison unavailable'
          : isAheadOfStable
            ? 'Ahead of latest stable'
            : 'Matches latest stable'
  const versionMessage = isVersionLoading
    ? 'Retrieving installed version and latest stable release information.'
    : isVersionUnavailable
      ? 'One or more version endpoints could not be loaded.'
      : hasNewerVersion
        ? 'A stable release newer than the installed version is ready to review.'
        : isComparisonUnavailable
          ? 'A valid installed version and stable release are needed to compare versions.'
          : isAheadOfStable
            ? 'The installed version is newer than the latest stable release. No downgrade is needed.'
            : 'The installed version matches the latest stable release.'

  return (
    <Box
      sx={{
        borderColor: 'divider',
        borderLeft: { xs: 0, md: 1 },
        borderTop: { xs: 1, md: 0 },
        height: '100%',
        mt: { xs: 1, md: 0 },
        pl: { xs: 0, md: 4 },
        pt: { xs: 3, md: 0 },
      }}
    >
      <Typography
        variant="overline"
        color="text.secondary"
        sx={{ display: 'block', letterSpacing: 0, mb: 1 }}
      >
        Version status
      </Typography>

      <Alert
        severity={versionSeverity}
        variant="outlined"
        sx={{ mb: 2, textAlign: 'left' }}
      >
        <AlertTitle sx={{ mb: 0.25 }}>{versionTitle}</AlertTitle>
        {versionMessage}
      </Alert>

      <Stack
        direction={{ xs: 'row', sm: 'row', md: 'column' }}
        divider={<Divider flexItem orientation="vertical" />}
        spacing={2}
        sx={{ mb: 2 }}
      >
        <Box>
          <VersionMetric
            label="Installed version"
            value={currentVersionLabel}
          />
          {isInstalledPrerelease && (
            <Typography variant="caption" color="text.secondary">
              Prerelease
            </Typography>
          )}
        </Box>
        <VersionMetric
          label="Latest stable release"
          value={latestVersionLabel}
        />
      </Stack>

      <Typography variant="body2" sx={{ mb: 1 }}>
        {releaseSource ? (
          <>
            Release source:{' '}
            <Link href={releaseSource.url}>{releaseSource.name}</Link>
          </>
        ) : (
          'Release source unavailable'
        )}
      </Typography>
      <Typography
        variant="caption"
        color="text.secondary"
        sx={{ display: 'block', mb: 2 }}
      >
        Release checks use stable releases from this source. Development builds
        and forks can include changes beyond their version label.
      </Typography>

      {hasNewerVersion && releaseUrl && (
        <Button
          color="warning"
          component={Link}
          endIcon={<OpenInNewIcon />}
          href={releaseUrl}
          variant="contained"
        >
          View release
        </Button>
      )}
    </Box>
  )
}

const PartnerLogoCard = ({ partner }: { partner: Partner }) => (
  <Grid item xs={6} sm={4} md={3} lg={2}>
    <Paper
      variant="outlined"
      sx={{
        alignItems: 'center',
        bgcolor: 'background.paper',
        display: 'flex',
        height: { xs: 112, sm: 132 },
        justifyContent: 'center',
        position: 'relative',
      }}
    >
      <Image
        src={partner.path}
        alt={partner.title}
        style={{ objectFit: 'contain', padding: '18px' }}
        sizes="(max-width: 600px) 45vw, (max-width: 900px) 30vw, 170px"
        fill
      />
    </Paper>
  </Grid>
)

const ContributorSection = ({ title, partners }: PartnerSection) => (
  <Box component="section">
    <Stack sx={{ mb: 2 }}>
      <Typography variant="h5" fontWeight={700}>
        {title}
      </Typography>
    </Stack>
    <Grid container spacing={2.5}>
      {partners.map((partner) => (
        <PartnerLogoCard key={partner.title} partner={partner} />
      ))}
    </Grid>
  </Box>
)

const About = () => {
  const {
    data: currentVersion,
    isLoading: isCurrentVersionLoading,
    isError: isCurrentVersionError,
  } = useGetVersionCurrentVersion(undefined, {
    query: {
      staleTime: 5 * 60 * 1000,
      retry: 1,
    },
  })
  const {
    data: latestVersion,
    isLoading: isLatestVersionLoading,
    isError: isLatestVersionError,
  } = useGetVersionLatestVersionFromPreRelease(false, undefined, {
    query: {
      enabled: true,
      staleTime: 5 * 60 * 1000,
      retry: 1,
    },
  })

  const isVersionLoading = isCurrentVersionLoading || isLatestVersionLoading
  const isVersionUnavailable = isCurrentVersionError || isLatestVersionError
  const currentVersionLabel = isCurrentVersionLoading
    ? 'Loading...'
    : isCurrentVersionError
      ? 'Unavailable'
      : getReleaseLabel(currentVersion)
  const latestVersionLabel = isLatestVersionLoading
    ? 'Loading...'
    : isLatestVersionError
      ? 'Unavailable'
      : getReleaseLabel(latestVersion)

  const installed = parseReleaseVersion(currentVersion)
  const stable = parseReleaseVersion(latestVersion)
  const comparison =
    installed &&
    stable &&
    !stable.prerelease.length &&
    !latestVersion?.prerelease &&
    !latestVersion?.draft
      ? compareVersions(installed, stable)
      : null
  const canCompare =
    !isVersionLoading && !isVersionUnavailable && comparison !== null
  const hasNewerVersion = canCompare && comparison < 0
  const releaseSource = getReleaseSource(latestVersion)
  const releaseUrl = releaseSource
    ? getReleaseSource({ htmlUrl: latestVersion?.htmlUrl })?.url ===
      releaseSource.url
      ? latestVersion?.htmlUrl
      : `${releaseSource.url}/releases`
    : undefined

  return (
    <ResponsivePageLayout title="About" hideTitle>
      <Box
        sx={{
          maxWidth: 1120,
          mx: 'auto',
          pb: 5,
          px: { xs: 2, sm: 3 },
          width: '100%',
        }}
      >
        <Paper
          component="section"
          variant="outlined"
          sx={{
            mb: 6,
            p: { xs: 3, md: 4 },
          }}
        >
          <Grid container spacing={{ xs: 3, md: 4 }} alignItems="stretch">
            <Grid item xs={12} md={7}>
              <Stack spacing={2.5}>
                <Box>
                  <Typography
                    variant="overline"
                    color="text.secondary"
                    sx={{ display: 'block', letterSpacing: 0, mb: 0.5 }}
                  >
                    Automated Traffic Signal Performance Measures
                  </Typography>
                  <Typography
                    variant="h2"
                    sx={{
                      fontSize: { xs: '2rem', md: '2.75rem' },
                      lineHeight: 1.12,
                    }}
                  >
                    About ATSPM
                  </Typography>
                </Box>

                <Typography
                  variant="subtitle1"
                  color="text.secondary"
                  sx={{ maxWidth: 680 }}
                >
                  Automated Traffic Signal Performance Measures show real-time
                  and historical functionality at signalized intersections. This
                  allows traffic engineers to directly measure what previously
                  could only be estimated and modeled.
                </Typography>

                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5}>
                  <Button
                    variant="contained"
                    href="/faq"
                    component={Link}
                    endIcon={<KeyboardDoubleArrowRightIcon />}
                  >
                    View FAQs
                  </Button>
                  <Button
                    variant="outlined"
                    href="https://github.com/OpenSourceTransportation/Atspm/blob/main/CHANGELOG.md"
                    component={Link}
                    endIcon={<OpenInNewIcon />}
                  >
                    View changelog
                  </Button>
                </Stack>
              </Stack>
            </Grid>

            <Grid item xs={12} md={5}>
              <VersionStatus
                currentVersionLabel={currentVersionLabel}
                latestVersionLabel={latestVersionLabel}
                hasNewerVersion={hasNewerVersion}
                isAheadOfStable={canCompare && comparison > 0}
                isComparisonUnavailable={comparison === null}
                isInstalledPrerelease={Boolean(
                  installed?.prerelease.length || currentVersion?.prerelease
                )}
                releaseSource={releaseSource}
                isVersionLoading={isVersionLoading}
                isVersionUnavailable={isVersionUnavailable}
                releaseUrl={releaseUrl}
              />
            </Grid>
          </Grid>
        </Paper>

        <Box component="section" sx={{ mb: 6, textAlign: 'center' }}>
          <Typography
            variant="overline"
            color="text.secondary"
            sx={{ display: 'block', letterSpacing: 0, mb: 1 }}
          >
            Presented by
          </Typography>
          <Box
            sx={{
              height: { xs: 116, sm: 150 },
              maxWidth: 620,
              mx: 'auto',
              position: 'relative',
            }}
          >
            <Image
              src={udotLogo.path}
              alt={udotLogo.title}
              style={{ objectFit: 'contain' }}
              sizes="(max-width: 900px) 90vw, 620px"
              fill
            />
          </Box>
        </Box>

        <Stack spacing={4}>
          <Box textAlign="center">
            <Typography variant="h4" sx={{ mb: 1 }}>
              Contributors
            </Typography>
            <Typography variant="body1" color="text.secondary">
              Organizations that have contributed to ATSPM.
            </Typography>
          </Box>

          {partnerSections.map((section) => (
            <ContributorSection key={section.title} {...section} />
          ))}
        </Stack>
      </Box>
    </ResponsivePageLayout>
  )
}

export default About
