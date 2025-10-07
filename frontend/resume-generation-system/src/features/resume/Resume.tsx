import styles from './Resume.module.css';
import { useParams } from 'react-router-dom';
import { useEffect, useState } from 'react';

type Job = {
  title: string;
  company: string;
  location: string;
  start: string;
  end: string;
  bullets: string[];
};

type Project = {
  name: string;
  description: string;
  technologies: string[];
  when: string;
};

type Education = {
  degree: string;
  school: string;
  location: string;
  graduation: string;
};

type ResumeData = {
  name: string;
  title: string;
  about: string;
  city: string;
  state: string;
  contact: {
    email: string;
    phone: string;
    github: string;
  };
  jobs: Job[];
  projects: Project[];
  education: Education[];
  skills: {
    label: string;
    items: string[];
  }[];
}

const BACKEND_URL = import.meta.env.VITE_BACKEND_URL;

export const Resume = () => {
  const { postingId } = useParams();

  const [resumeData, setResumeData] = useState<ResumeData | undefined>(undefined);

  useEffect(() => {
    let ignore = false;

    const fetchItems = async () => {
      try {
        const response = await fetch(`${BACKEND_URL}/api/GetResumeData?postingId=${postingId}`,);
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        console.log(data);
        if (!ignore) setResumeData(data);
      } catch (error) {
        console.log(error);
      }
    };

    fetchItems();

    return () => { ignore = true };
  }, [postingId]);

  if (!resumeData) {
    return <div>Loading...</div>;
  }

  return (<article className={styles.resume}>
    <ResumeHeader resumeData={resumeData} />
    <ExperienceSection resumeData={resumeData} />
    <ProjectsSection resumeData={resumeData} />
    <EducationSection resumeData={resumeData} />
    <ContactSection resumeData={resumeData} />
    <SkillsSection resumeData={resumeData} />
  </article>);
}

const ExperienceSection = ({ resumeData }: { resumeData: ResumeData }) => {
  return (<section className={styles.experienceSection}>
    <h2>Experience</h2>
    {resumeData.jobs.map((job, index) => (<Job key={index} job={job} />))}
  </section>)
};

const ProjectsSection = ({ resumeData }: { resumeData: ResumeData }) => {
  return (<section className={styles.projectsSection}>
    <h2>Projects</h2>
    {resumeData.projects.map((project, index) => (<Project key={index} project={project} />))}
  </section>)
};

const EducationSection = ({ resumeData }: { resumeData: ResumeData }) => {
  return (<section className={styles.educationSection}>
    <h2>Education</h2>
    {resumeData.education.map((edu, index) => (<Education key={index} education={edu} />))}
  </section>)
};

const ResumeHeader = ({ resumeData }: { resumeData: ResumeData }) => {
  return (<header className={styles.header}>
    <h1>{resumeData.name}</h1>
    <h2>{resumeData.title}</h2>
    <p>{resumeData.about}</p>
  </header>);
}

const ContactSection = ({ resumeData }: { resumeData: ResumeData }) => {
  return (<section className={styles.contactSection}>
    <p className={styles.address}>{resumeData.city}, {resumeData.state}</p>
    <p className={styles.gmail}>{resumeData.contact.email}</p>
    <p className={styles.phone}>{resumeData.contact.phone}</p>
    <p className={styles.github}>{resumeData.contact.github}</p>
  </section>)
}

const Job = ({ job }: { job: Job }) => {
  return (<section>
    <h3 className={styles.full}><span className={styles.left}>{job.title}</span><span className={styles.right}>{job.company}</span></h3>
    <p className={styles.full}><span className={styles.left}>{job.start} - {job.end}</span><span className={styles.right}>{job.location}</span></p>
    <ul>
      {job.bullets.map((bullet, index) => (<li key={index}>{bullet}</li>))}
    </ul>
  </section>);
}

const Project = ({ project }: { project: Project }) => {
  return (<section>
    <div className={styles.full}>
      <h3 className={styles.left}>{project.name}</h3><p className={styles.right}>{project.when}</p>
    </div>
    <p>{project.description}</p>
    <p><strong>Technologies:</strong> {project.technologies.join(', ')}</p>
  </section>);
}

const Education = ({ education }: { education: Education }) => {
  return (<div className={styles.full}>
    <h3 className={styles.left}>{education.degree} - {education.school}</h3>
    <p className={styles.right}>{education.location} | {education.graduation}</p>
  </div>);
}

const SkillsSection = ({ resumeData }: { resumeData: ResumeData }) => {
  return (<section className={styles.skillsSection}>
    <h2>Skills</h2>
    {resumeData.skills.map((area, index) => (
      <section key={index}>
        <h3>{area.label}</h3>
        <ul>{area.items.map((item, index) => (
          <li key={index}>{item}</li>
        ))}
        </ul>
      </section>))}
  </section>);
}