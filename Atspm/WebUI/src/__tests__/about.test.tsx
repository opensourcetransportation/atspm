import type { GitHubReleaseDto } from '@/api/config'
import {
  useGetVersionCurrentVersion,
  useGetVersionLatestVersionFromPreRelease,
} from '@/api/config'
import About from '@/pages/about'
import '@testing-library/jest-dom'
import { render, screen } from '@testing-library/react'

jest.mock('@/api/config', () => ({
  __esModule: true,
  useGetVersionCurrentVersion: jest.fn(),
  useGetVersionLatestVersionFromPreRelease: jest.fn(),
}))

const mockUseGetVersionCurrentVersion = useGetVersionCurrentVersion as jest.Mock
const mockUseGetVersionLatestVersionFromPreRelease =
  useGetVersionLatestVersionFromPreRelease as jest.Mock

const release = (
  tagName: string,
  htmlUrl = `https://github.com/OpenSourceTransportation/Atspm/releases/tag/${tagName}`
): GitHubReleaseDto => ({
  tagName,
  htmlUrl,
})

const mockVersionHooks = ({
  currentVersion,
  latestVersion,
  isCurrentVersionError = false,
  isLatestVersionError = false,
  isCurrentVersionLoading = false,
  isLatestVersionLoading = false,
}: {
  currentVersion?: GitHubReleaseDto
  latestVersion?: GitHubReleaseDto
  isCurrentVersionError?: boolean
  isLatestVersionError?: boolean
  isCurrentVersionLoading?: boolean
  isLatestVersionLoading?: boolean
}) => {
  mockUseGetVersionCurrentVersion.mockReturnValue({
    data: currentVersion,
    isError: isCurrentVersionError,
    isLoading: isCurrentVersionLoading,
  })
  mockUseGetVersionLatestVersionFromPreRelease.mockReturnValue({
    data: latestVersion,
    isError: isLatestVersionError,
    isLoading: isLatestVersionLoading,
  })
}

describe('About page', () => {
  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('shows an update prompt when the latest release is newer', () => {
    mockVersionHooks({
      currentVersion: release('v5.2.0'),
      latestVersion: release('v5.2.1'),
    })

    render(<About />)

    expect(
      screen.getByRole('heading', { name: 'About ATSPM' })
    ).toBeInTheDocument()
    expect(screen.getByText('Update available')).toBeInTheDocument()
    expect(screen.getByText('v5.2.0')).toBeInTheDocument()
    expect(screen.getByText('v5.2.1')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /view release/i })).toHaveAttribute(
      'href',
      'https://github.com/OpenSourceTransportation/Atspm/releases/tag/v5.2.1'
    )
    expect(mockUseGetVersionLatestVersionFromPreRelease).toHaveBeenCalledWith(
      false,
      undefined,
      expect.objectContaining({
        query: expect.objectContaining({ enabled: true }),
      })
    )
  })

  it('treats matching versions as up to date after normalizing the tag prefix', () => {
    mockVersionHooks({
      currentVersion: release('5.2.1'),
      latestVersion: release('v5.2.1'),
    })

    render(<About />)

    expect(screen.getByText('Matches latest stable')).toBeInTheDocument()
    expect(
      screen.queryByRole('link', { name: /view release/i })
    ).not.toBeInTheDocument()
  })

  it('shows an unavailable state when a version endpoint fails', () => {
    mockVersionHooks({
      currentVersion: undefined,
      latestVersion: release('v5.2.1'),
      isCurrentVersionError: true,
    })

    render(<About />)

    expect(screen.getByText('Version status unavailable')).toBeInTheDocument()
    expect(screen.getByText('Unavailable')).toBeInTheDocument()
    expect(screen.queryByText('Matches latest stable')).not.toBeInTheDocument()
  })

  it.each([
    ['v5.4.0-rc1', 'v5.3.1', 'Ahead of latest stable'],
    ['v5.4.0', 'v5.3.1', 'Ahead of latest stable'],
    ['v5.3.1', 'v5.3.1', 'Matches latest stable'],
    ['v5.3.1+fork.12', '5.3.1+upstream.4', 'Matches latest stable'],
    ['v5.4.0-rc1', 'v5.4.0', 'Update available'],
    ['v5.3.9', 'v5.3.10', 'Update available'],
    ['v5.9.0', 'v5.10.0', 'Update available'],
    ['v5.3.1', 'v6.0.0', 'Update available'],
    ['unknown', 'v5.3.1', 'Version comparison unavailable'],
    ['v5.3.1', 'release-not-a-version', 'Version comparison unavailable'],
    ['v5.3.1', 'v5.4.0-rc1', 'Version comparison unavailable'],
  ])('compares installed %s with stable %s', (installed, stable, title) => {
    mockVersionHooks({
      currentVersion: release(installed),
      latestVersion: release(stable),
    })
    render(<About />)
    expect(screen.getByText(title)).toBeInTheDocument()
    expect(screen.getByText('Installed version')).toBeInTheDocument()
    expect(screen.getByText('Latest stable release')).toBeInTheDocument()
    expect(Boolean(screen.queryByRole('link', { name: /view release/i }))).toBe(
      title === 'Update available'
    )
  })

  it('identifies the release source from the returned release URL', () => {
    mockVersionHooks({
      currentVersion: release('v5.4.0-rc1'),
      latestVersion: release(
        'v5.3.1',
        'https://github.com/utahudot/udot-atspm/releases/tag/v5.3.1'
      ),
    })
    render(<About />)
    expect(
      screen.getByRole('link', { name: 'utahudot/udot-atspm' })
    ).toHaveAttribute('href', 'https://github.com/utahudot/udot-atspm')
    expect(screen.getByText('Prerelease')).toBeInTheDocument()
  })

  it.each([
    'javascript:alert(1)',
    'https://example.org/owner/repo/releases/tag/v5.3.1',
  ])('does not guess a source from %s', (url) => {
    mockVersionHooks({
      currentVersion: release('5.3.1'),
      latestVersion: release('5.3.1', url),
    })
    render(<About />)
    expect(screen.getByText('Release source unavailable')).toBeInTheDocument()
  })

  it('does not report an update while release data is loading', () => {
    mockVersionHooks({
      currentVersion: release('5.2.0'),
      latestVersion: release('5.3.1'),
      isLatestVersionLoading: true,
    })
    render(<About />)
    expect(screen.getByText('Checking version status')).toBeInTheDocument()
    expect(screen.queryByText('Update available')).not.toBeInTheDocument()
  })

  it.each([{ draft: true }, { prerelease: true }])(
    'rejects a non-stable release marked %j',
    (flags) => {
      mockVersionHooks({
        currentVersion: release('5.2.0'),
        latestVersion: { ...release('5.3.1'), ...flags },
      })
      render(<About />)
      expect(
        screen.getByText('Version comparison unavailable')
      ).toBeInTheDocument()
      expect(
        screen.queryByRole('link', { name: /view release/i })
      ).not.toBeInTheDocument()
    }
  )

  it.each([
    [undefined, release('5.3.1')],
    [release('5.3.1'), undefined],
  ])(
    'does not claim a match when release data is missing',
    (currentVersion, latestVersion) => {
      mockVersionHooks({ currentVersion, latestVersion })
      render(<About />)
      expect(
        screen.getByText('Version comparison unavailable')
      ).toBeInTheDocument()
      expect(
        screen.queryByText('Matches latest stable')
      ).not.toBeInTheDocument()
    }
  )

  it('does not expose a release button for an invalid source URL', () => {
    mockVersionHooks({
      currentVersion: release('5.2.0'),
      latestVersion: release('5.3.1', 'javascript:alert(1)'),
    })
    render(<About />)
    expect(screen.getByText('Update available')).toBeInTheDocument()
    expect(
      screen.queryByRole('link', { name: /view release/i })
    ).not.toBeInTheDocument()
  })

  it('shows the configured repository when release metadata has no tag or URL', () => {
    mockVersionHooks({
      currentVersion: { name: 'v5.4.0-rc1' },
      latestVersion: {
        name: 'v5.3.1',
        sourceRepositoryUrl: 'https://github.com/utahudot/udot-atspm',
      },
    })
    render(<About />)
    expect(screen.getByText('Ahead of latest stable')).toBeInTheDocument()
    expect(
      screen.getByRole('link', { name: 'utahudot/udot-atspm' })
    ).toBeInTheDocument()
  })

  it('links to configured releases when a newer release has no individual URL', () => {
    mockVersionHooks({
      currentVersion: release('5.2.0'),
      latestVersion: {
        name: '5.3.1',
        sourceRepositoryUrl: 'https://github.com/example/atspm',
      },
    })
    render(<About />)
    expect(screen.getByRole('link', { name: /view release/i })).toHaveAttribute(
      'href',
      'https://github.com/example/atspm/releases'
    )
  })
})
