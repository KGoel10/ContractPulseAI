import { useState } from 'react'
import './App.css'

const navItems = [
  { id: 'dashboard', label: 'Dashboard', icon: 'DB' },
  { id: 'initiation', label: 'Project Initiation', icon: 'PI' },
  { id: 'preview', label: 'RFP Response Preview', icon: 'RP' },
  { id: 'rates', label: 'Rate Benchmarking', icon: 'RB' },
  { id: 'risk', label: 'Legal Risk Audit', icon: 'LR' },
  { id: 'history', label: 'Deal Version History', icon: 'DH' },
]

const kpis = [
  { label: 'RFP Status', value: '18 Active' },
  { label: 'Old SOW Library', value: '146 Files' },
  { label: 'Recent RFPs', value: '4 New Today' },
]

const previewLines = [
  'Develop a cloud-native RFP processing platform to manage cloud alignment technology, provider analysis, and controlled drafting workflows.',
  'Approved commercial values such as [RATE_CARD_SECURE_A] and [RATE_CARD_SECURE_B] are applied only after verification.',
  'All client-sensitive terms remain tokenized until account manager approval is completed.',
  'Generate a draft SOW and replace [CLIENT_ENTITY_1] only during the final approved export step.',
]

const sowItems = [
  {
    id: 'SOW-2401',
    client: 'Northwind Health',
    title: 'Cloud migration advisory and managed operations',
    status: 'Ongoing',
    description:
      'Prepare migration architecture, establish cost guardrails, and define transition plan for quarterly execution.',
    prompt:
      'Generate draft SOW with privacy token placeholders. Keep client entities tokenized until final approved export.',
  },
  {
    id: 'SOW-2379',
    client: 'BluePeak Energy',
    title: 'RFP program modernization with legal review',
    status: 'Completed',
    description:
      'Program completed with final legal approval and signed commercial terms for production deployment.',
    prompt:
      'Generate final approved SOW output for archive view and compliance records.',
  },
  {
    id: 'SOW-2410',
    client: 'Apex Mobility',
    title: 'Rate-card alignment and managed services setup',
    status: 'Ongoing',
    description:
      'Create benchmark model, align rate cards to policy, and prepare anonymized draft for account team review.',
    prompt:
      'Generate a tokenized working draft and keep placeholders for secure rate card references.',
  },
]

function App() {
  const [activeView, setActiveView] = useState('dashboard')
  const [requirementText, setRequirementText] = useState(
    'Develop a cloud-native RFP processing platform to manage cloud alignment technology, rate-card comparisons, provider analysis, and compliant document generation.',
  )
  const [promptText, setPromptText] = useState(
    'Generate a draft SOW. Replace [CLIENT_ENTITY_1] with client name during final export, keep tokenized placeholders intact, and avoid exposing sensitive data.',
  )

  const handleEditOngoingSow = (item) => {
    setRequirementText(item.description)
    setPromptText(item.prompt)
    setActiveView('initiation')
  }

  const handleOpenPreview = (item) => {
    setRequirementText(item.description)
    setPromptText(item.prompt)
    setActiveView('preview')
  }

  return (
    <main className="shell">
      <aside className="sidebar">
        <div className="brand">
          <div className="brandMark" aria-hidden="true">
            <img src="/favicon.svg" alt="" className="brandLogo" />
          </div>
          <h1>
            ContractPulse <span>AI</span>
          </h1>
        </div>

        <nav className="navList" aria-label="Application sections">
          {navItems.map((item) => (
            <button
              key={item.id}
              className={item.id === activeView ? 'navItem active' : 'navItem'}
              onClick={() => setActiveView(item.id)}
              type="button"
            >
              <span className="navIcon" aria-hidden="true">{item.icon}</span>
              <span>{item.label}</span>
            </button>
          ))}
        </nav>
      </aside>

      <section className="workspace">
        <header className="topbar">
          <div className="topStatus">Privacy Shield: ACTIVE</div>

          <div className="profilePill">
            <span className="avatar" aria-hidden="true">AD</span>
            <div>
              <strong>Admin</strong>
            </div>
          </div>
        </header>

        {activeView === 'dashboard' && (
          <section className="page dashboardPage">
            <article className="panel dashboardPanel">
              <div className="pageTitleRow">
                <div>
                  <h2>Active SOW Dashboard</h2>
                </div>
                <span className="tagPill">Status Tracking</span>
              </div>

              <div className="dashboardTable">
                <div className="dashboardHeaderRow">
                  <span>SOW</span>
                  <span>Client</span>
                  <span>Status</span>
                  <span>Action</span>
                </div>

                {sowItems.map((item) => (
                  <div key={item.id} className="dashboardRow">
                    <div>
                      <strong>{item.id}</strong>
                      <p>{item.title}</p>
                    </div>
                    <span>{item.client}</span>
                    <span className={item.status === 'Ongoing' ? 'statusTag ongoing' : 'statusTag completed'}>
                      {item.status}
                    </span>
                    <div className="rowActions">
                      {item.status === 'Ongoing' ? (
                        <button type="button" className="secondaryButton" onClick={() => handleEditOngoingSow(item)}>
                          Edit SOW
                        </button>
                      ) : (
                        <button type="button" className="secondaryButton" onClick={() => handleOpenPreview(item)}>
                          Open Preview
                        </button>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </article>
          </section>
        )}

        {activeView === 'initiation' && (
          <section className="page intakePage">
            <article className="panel intakePanel">
              <div className="pageTitleRow">
                <div>
                  <h2>Requirement Intake & Configuration</h2>
                </div>
                <div className="sectionIcon">KB</div>
              </div>

              <label className="field">
                <span>Higher-level requirements / SOW description</span>
                <textarea value={requirementText} onChange={(event) => setRequirementText(event.target.value)} />
              </label>

              <label className="field">
                <span>Prompt or instructions for anonymized generation</span>
                <textarea value={promptText} onChange={(event) => setPromptText(event.target.value)} />
              </label>

              <div className="actionRow">
                <button className="primaryButton" type="button" onClick={() => setActiveView('preview')}>
                  Process requirements & prepare tokenized draft
                </button>
              </div>

              <div className="kpiGrid">
                {kpis.map((item) => (
                  <article key={item.label} className="kpiCard">
                    <span>{item.label}</span>
                    <strong>{item.value}</strong>
                  </article>
                ))}
              </div>
            </article>
          </section>
        )}

        {activeView === 'preview' && (
          <section className="page previewPage">
            <div className="previewLayout">
              <article className="panel previewPanel">
                <div className="pageTitleRow">
                  <div>
                    <h2>Generated RFP Draft Sandbox</h2>
                  </div>
                  <span className="tagPill">Anonymized Token View</span>
                </div>

                <input className="searchBar" defaultValue="Edit your text input here" aria-label="Edit draft" />

                <div className="documentBody">
                  <p className="documentGreeting">Dear [CLIENT_ENTITY_1],</p>
                  {previewLines.map((line) => (
                    <p key={line} className="documentLine">{line}</p>
                  ))}
                </div>
              </article>

              <aside className="sideStack">
                <article className="panel sideCard">
                  <h3>RAG Context Derived:</h3>
                  <p>(Historical Assets)</p>
                </article>
                <article className="panel sideCard success">
                  <h3>System Verification Match:</h3>
                  <strong>94% Aligned</strong>
                </article>
              </aside>
            </div>
          </section>
        )}

        {activeView === 'rates' && (
          <section className="page ratesPage">
            <article className="panel">
              <h2>Rate Benchmarking</h2>
              <div className="benchmarkGrid">
                <div className="benchmarkCard">
                  <span>Consulting</span>
                  <strong>$180/hr</strong>
                </div>
                <div className="benchmarkCard">
                  <span>Implementation</span>
                  <strong>$220/hr</strong>
                </div>
                <div className="benchmarkCard">
                  <span>Managed services</span>
                  <strong>$145/hr</strong>
                </div>
              </div>
            </article>
          </section>
        )}

        {activeView === 'risk' && (
          <section className="page riskPage">
            <article className="panel">
              <h2>Legal Risk Audit</h2>
              <div className="historyTimeline">
                <div>
                  <strong>High Priority</strong>
                  <p>Payment terms exceed approved policy threshold in two sections.</p>
                </div>
                <div>
                  <strong>Medium Priority</strong>
                  <p>Indemnity clause contains unapproved alternate language.</p>
                </div>
                <div>
                  <strong>Low Priority</strong>
                  <p>Formatting differences detected in signature section.</p>
                </div>
              </div>
            </article>
          </section>
        )}

        {activeView === 'history' && (
          <section className="page historyPage">
            <article className="panel">
              <h2>Deal Version History</h2>
              <div className="historyTimeline">
                <div>
                  <strong>v1</strong>
                  <p>Initial draft generated from tokenized RFP intake.</p>
                </div>
                <div>
                  <strong>v2</strong>
                  <p>Client redlines applied and flagged sections sent to legal review.</p>
                </div>
                <div>
                  <strong>v3</strong>
                  <p>Final approval captured before export to Word and PDF.</p>
                </div>
              </div>
            </article>
          </section>
        )}
      </section>
    </main>
  )
}

export default App
