import React from 'react';
import clsx from 'clsx';
import styles from './styles.module.css';

function MessageIcon() {
  return (
    <svg
      className={styles.featureSvg}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z" />
      <line x1="8" y1="9" x2="16" y2="9" />
      <line x1="8" y1="13" x2="14" y2="13" />
    </svg>
  );
}

function ShieldCheckIcon() {
  return (
    <svg
      className={styles.featureSvg}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />
      <polyline points="9 12 11 14 15 10" />
    </svg>
  );
}

function ShareIcon() {
  return (
    <svg
      className={styles.featureSvg}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <circle cx="18" cy="5" r="3" />
      <circle cx="6" cy="12" r="3" />
      <circle cx="18" cy="19" r="3" />
      <line x1="8.59" y1="13.51" x2="15.42" y2="17.49" />
      <line x1="15.41" y1="6.51" x2="8.59" y2="10.49" />
    </svg>
  );
}

type FeatureItem = {
  title: string;
  Icon: React.ComponentType;
  description: React.JSX.Element;
};

const FeatureList: FeatureItem[] = [
  {
    title: 'Unified Message Model',
    Icon: MessageIcon,
    description: (
      <>
        One <code>IMessage</code>, one <code>IChannelConnector</code>, one
        consistent programming model &mdash; the same abstractions for SMS,
        email, push notifications, and chat. No provider lock-in, no SDK
        fragmentation.
      </>
    ),
  },
  {
    title: 'Schema-Driven Validation',
    Icon: ShieldCheckIcon,
    description: (
      <>
        Every connector declares its capabilities and constraints via{' '}
        <code>IChannelSchema</code>. Validate messages before they reach the
        provider &mdash; catch invalid payloads at build time, not at send
        time.
      </>
    ),
  },
  {
    title: 'Extensible by Design',
    Icon: ShareIcon,
    description: (
      <>
        Extend the framework with custom connectors for any provider &mdash;
        social messaging (Slack, Teams, WhatsApp), protocol-level channels
        (SMPP, SMTP, APNs), or proprietary gateways. One base class, any
        transport.
      </>
    ),
  },
];

function Feature({ title, Icon, description }: FeatureItem) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center">
        <Icon />
      </div>
      <div className="text--center padding-horiz--md">
        <h3>{title}</h3>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures(): React.JSX.Element {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
