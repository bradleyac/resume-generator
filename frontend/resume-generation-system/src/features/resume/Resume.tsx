import { resumeData } from './resumeData';
import styles from './Resume.module.css';

export const Resume = () => {
  return (<article className={styles.resume}>
    <ResumeHeader />
    <ExperienceSection />
    <ProjectsSection />
    <EducationSection />
    <ContactSection />
    <SkillsSection />
  </article>);
}

const ExperienceSection = () => {
  return (<section className={styles.experienceSection}>
    <h2>Experience</h2>
    {resumeData.jobs.map((job, index) => (<Job key={index} job={job} />))}
  </section>)
};

const ProjectsSection = () => {
  return (<section className={styles.projectsSection}>
    <h2>Projects</h2>
    {resumeData.projects.map((project, index) => (<Project key={index} project={project} />))}
  </section>)
};

const EducationSection = () => {
  return (<section className={styles.educationSection}>
    <h2>Education</h2>
    {resumeData.education.map((edu, index) => (<Education key={index} education={edu} />))}
  </section>)
};

const ResumeHeader = () => {
  return (<header className={styles.header}>
    <h1>{resumeData.name}</h1>
    <h2>{resumeData.title}</h2>
    <p>{resumeData.about}</p>
  </header>);
}

const ContactSection = () => {
  return (<section className={styles.contactSection}>
    <p>{resumeData.city}, {resumeData.state}</p>
    <p className={styles.gmail}>{resumeData.contact.email}</p>
    <p>{resumeData.contact.phone}</p>
    <p className={styles.github}>{resumeData.contact.github}</p>
  </section>)
}

const Job = ({ job }: { job: typeof resumeData.jobs[0] }) => {
  return (<section>
    <h3 className={styles.full}><span className={styles.left}>{job.title}</span><span className={styles.right}>{job.company}</span></h3>
    <p className={styles.full}><span className={styles.left}>{job.start} - {job.end}</span><span className={styles.right}>{job.location}</span></p>
    <ul>
      {job.bullets.slice(0, 10).map((bullet, index) => (<li key={index}>{bullet}</li>))}
    </ul>
  </section>);
}

const Project = ({ project }: { project: typeof resumeData.projects[0] }) => {
  return (<section>
    <h3 className={styles.full}><span className={styles.left}>{project.name}</span><span className={styles.right}>{project.when}</span></h3>
    <p>{project.description}</p>
    <p><strong>Technologies:</strong> {project.technologies.join(', ')}</p>
  </section>);
}

const Education = ({ education }: { education: typeof resumeData.education[0] }) => {
  return (<div>
    <h3>{education.degree} - {education.school}</h3>
    <p>{education.location} | {education.graduation}</p>
  </div>);
}

const SkillsSection = () => {
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