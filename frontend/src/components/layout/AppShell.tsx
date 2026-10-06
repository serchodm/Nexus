import { Outlet } from 'react-router-dom';
import { Sidebar } from './Sidebar';
import { TopBar } from './TopBar';
import { AuraDrawer } from './AuraDrawer';
import { MobileBottomNav } from './MobileBottomNav';

/**
 * AppShell: Full application layout with sidebar, top bar, main content, and Aura AI drawer.
 * Responsive: Desktop shows sidebar + aura drawer, mobile shows bottom nav.
 */
export function AppShell() {
  return (
    <div className="app-shell">
      {/* Left Navigation Sidebar (Desktop only) */}
      <Sidebar />

      {/* Main workspace container */}
      <div className="app-shell__workspace">
        {/* Consolidated Header / Top Bar */}
        <TopBar />

        {/* Body: Main Content + Aura Drawer */}
        <div className="app-shell__body">
          {/* Center Scrollable Content */}
          <main className="app-shell__main">
            <Outlet />
          </main>

          {/* Right Drawer: Nexus Aura AI Copilot (Desktop only) */}
          <AuraDrawer />
        </div>
      </div>

      {/* Mobile Bottom Navigation */}
      <MobileBottomNav />
    </div>
  );
}
